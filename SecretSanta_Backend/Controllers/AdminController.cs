using Microsoft.AspNetCore.Mvc;
using SecretSanta_Backend.Services;
using SecretSanta_Backend.Models;
using SecretSanta_Backend.ModelsDTO;
using SecretSanta_Backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace SecretSanta_Backend.Controllers
{
    [ApiController]
    [Route("event")]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private IRepositoryWrapper _repository;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger, IRepositoryWrapper repository)
        {
            _logger = logger;
            _repository = repository;
        }

        /// <summary>
        /// Создать новую игру.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] EventCreate @event)
        {
            try
            {
                if (@event is null)
                {
                    _logger.LogError("Event object recived from client is null.");
                    return BadRequest(new { message = "Null object" });
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Event object recived from client is not valid.");
                    return BadRequest(new { message = "Invalid object" });
                }

                var eventId = Guid.NewGuid();
                var eventResult = new Event
                {
                    Id = eventId,
                    Description = @event.Description,
                    EndEvent = @event.EndEvent.SetKindUtc(),
                    EndRegistration = @event.EndRegistration.SetKindUtc(),
                    SumPrice = @event.Sumprice,
                    SendFriends = @event.Sendfriends,
                    Tracking = @event.Tracking,
                    Reshuffle = false
                };

                _repository.Event.CreateEvent(eventResult);
                await _repository.SaveAsync();

                return Ok(eventResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside CreateEvent action: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Удалить игру.
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpDelete("{eventId}")]
        public async Task<IActionResult> DeleteEvent(Guid eventId)
        {
            try
            {
                var @event = await _repository.Event.FindByCondition(x => x.Id == eventId).FirstOrDefaultAsync();

                if (@event is null)
                {
                    _logger.LogError($"Event with ID: {eventId} not found");
                    return BadRequest(new { message = "Event not found" });
                }
                if (@event.Reshuffle == true)
                {
                    _logger.LogError("Registration date has already expired");
                    return BadRequest(new { message = "Registration date has already expired" });
                }

                _repository.Event.DeleteEvent(@event);
                await _repository.SaveAsync();

                //return NoContent();
                return StatusCode(200, "{}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Incorrectly passed ID argument: { ex.Message}.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Получить список существующих игр.
        /// </summary>
        /// <returns></returns>
        [HttpGet("events")]
        public async Task<ActionResult<EventViewList>> GetEvents()
        {
            var events = await _repository.Event.FindAll().Select(x => new EventViewList(){Id = x.Id,Description = x.Description}).ToListAsync();
            if (events is null)
                return BadRequest(new { message = "Events does not exist." });

            return Ok(events);
        }

        /// <summary>
        /// Получить информацию об игре.
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpGet("{eventId}")]
        public async Task<ActionResult<EventView>> GetEventById(Guid eventId)
        {
            try
            { 
                if (eventId == Guid.Empty)
                    return BadRequest(new { message = "Request argument omitted." });
                var @event = await _repository.Event.FindByCondition(x => x.Id == eventId).FirstOrDefaultAsync();
                if (@event is null)
                    return BadRequest(new { message = "Game with this Id does not exist." });

                var eventCount = await _repository.MemberEvent.FindByCondition(x => x.EventId == eventId).Select(x => x.EventId).CountAsync();

                var eventsMember = await _repository.MemberEvent.FindByCondition(x => x.EventId == eventId).ToListAsync();
                
                List<MemberViewAdmin> memberViewAdminList = new List<MemberViewAdmin>();
                foreach(var eventMember in eventsMember)
                {
                    var memberSearch = await _repository.Member.FindByCondition(x => x.Id == eventMember.MemberId).FirstOrDefaultAsync();
                    MemberView memberView = new MemberView();
                    if (memberSearch != null)
                    {
                        memberView.Surname = memberSearch.Surname;
                        memberView.Name = memberSearch.Name;
                        memberView.Patronymic = memberSearch.Patronymic;
                        memberView.Email = memberSearch.Email;
                    }

                    var memberRecipientSearch = await _repository.Member.FindByCondition(x => x.Id == eventMember.Recipient).FirstOrDefaultAsync();
                    MemberView memberRecipient = new MemberView();
                    if (memberRecipientSearch != null)
                    {
                        memberRecipient.Surname = memberRecipientSearch.Surname;
                        memberRecipient.Name = memberRecipientSearch.Name;
                        memberRecipient.Patronymic = memberRecipientSearch.Patronymic;
                        memberRecipient.Email = memberRecipientSearch.Email;

                    }

                    var memberSenderId = await _repository.MemberEvent.FindByCondition(x => x.Recipient == eventMember.MemberId && x.EventId == eventId).Select(x => x.MemberId).FirstOrDefaultAsync();
                    var memberSenderSearch = await _repository.Member.FindByCondition(x => x.Id == memberSenderId).FirstOrDefaultAsync();
                    MemberView memberSender = new MemberView();
                    if (memberSenderSearch != null)
                    {
                        memberSender.Surname = memberSenderSearch.Surname;
                        memberSender.Name = memberSenderSearch.Name;
                        memberSender.Patronymic = memberSenderSearch.Patronymic;
                        memberSender.Email = memberSenderSearch.Email;
                    }

                    MemberViewAdmin memberViewAdmin = new MemberViewAdmin
                    {
                        MemberView = memberView,
                        MemberRecipient = memberRecipient,
                        MemberSender = memberSender
                    };
                    memberViewAdminList.Add(memberViewAdmin);
                }
                
                EventView eventView = new EventView
                {
                    Id = eventId,
                    Description = @event.Description,
                    EndRegistration = @event.EndRegistration,
                    EndEvent = @event.EndEvent,
                    SumPrice = @event.SumPrice,
                    Tracking = @event.Tracking,
                    Reshuffle = @event.Reshuffle,
                    MembersCount = eventCount,
                    MemberView = memberViewAdminList
                };

                return Ok(eventView);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Incorrectly passed ID argument: { ex.Message}.");
                return StatusCode(500, new { message = "Internal server error" });

            }
        }

        /// <summary>
        /// Редактировать данные игры.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="event"></param>
        /// <returns></returns>
        [HttpPut("{eventId}")]
        public async Task<IActionResult> UpdateEventById(Guid eventId, [FromBody]EventCreate @event)
        {
            try
            {
                if (@event is null)
                {
                    _logger.LogError("Event object recived from client is null.");
                    return BadRequest(new { message = "Null object" });
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Event object recived from client is not valid.");
                    return BadRequest(new { message = "Invalid object" });
                }


                var eventResult = await _repository.Event.FindByCondition(x => x.Id == eventId).FirstOrDefaultAsync();
                if (eventResult is null)
                {
                    _logger.LogError("Event object not found.");
                    return BadRequest("Event not found");
                }
                if (eventResult.Reshuffle == true)
                {
                    _logger.LogError("Registration date has already expired");
                    return BadRequest(new { message = "Registration date has already expired" });
                }

                eventResult.Description = @event.Description;
                eventResult.EndRegistration = @event.EndRegistration.SetKindUtc();
                eventResult.EndEvent = @event.EndEvent.SetKindUtc();
                eventResult.SumPrice = @event.Sumprice;
                eventResult.Tracking = @event.Tracking;

                _repository.Event.UpdateEvent(eventResult);
                await _repository.SaveAsync();

                //return NoContent();
                return StatusCode(200, "{}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Incorrectly passed argument: { ex.Message}.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Получить список игр, в которых участвует пользователь.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("events/{memberId}")]
        public async Task<IActionResult> GetEventsByMember(Guid userId)
        {
            try
            {
                var events = _repository.Event.GetEventsByMemberId(userId);

                if (events == null)
                {
                    _logger.LogError("Member is not take part one more event");
                    return BadRequest(new { message = "Events not found" });
                }

                List<EventView> eventsList = new List<EventView>();

                foreach (var @event in events)
                {
                    EventView view = new EventView
                    {
                        Id = @event.Id,
                        Description = @event.Description,
                        EndRegistration = @event.EndRegistration,
                        EndEvent = @event.EndEvent,
                        SumPrice = @event.SumPrice,
                        Tracking = @event.Tracking
                    };
                    eventsList.Add(view);
                }

                return Ok(eventsList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetEventsList action: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    } 
}