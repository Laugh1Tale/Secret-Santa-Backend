using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SecretSanta_Backend.Models;
using SecretSanta_Backend.ModelsDTO;
using SecretSanta_Backend.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace SecretSanta_Backend.Controllers
{
    [ApiController]
    [Authorize(Roles = "admin,user")]
    [Route("user")]
    public class MemberController : ControllerBase
    {
        private IRepositoryWrapper _repository;
        private ILogger<MemberController> _logger;

        public MemberController(IRepositoryWrapper repository, ILogger<MemberController> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        /// <summary>
        /// Возвращает информацию об игре для конкретного пользователя.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="eventId"></param>
        /// <returns>Описание игры, дату окончания регистрации, дату последнего дня отправки посылки, примерную стоимость подарка,
        /// указанные предпочтения, количество участников в этой игре, статус проведения распределения участников.</returns>
        [HttpGet("{userId}/event/{eventId}")]
        public async Task<ActionResult<MemberEventView>> GetEventInfo(Guid userId, Guid eventId)
        {
            try
            {
                var @event = await _repository.Event.FindByCondition(x => x.Id == eventId).FirstOrDefaultAsync();
                if (@event is null)
                {
                    _logger.LogError("Event doesn't exist.");
                    return BadRequest(new { message = "Event doesn't exist" });
                }

                var eventPreferences = await _repository.MemberEvent.FindByCondition(x => x.MemberId == userId && x.EventId == eventId).FirstOrDefaultAsync();
                if (eventPreferences is null || eventPreferences.MemberAttend is false)
                {
                    _logger.LogError("Prefernces object is null.");
                    return BadRequest(new { message = "Member does not participate in the event" });
                }
                var memberAttendCount = await _repository.MemberEvent.FindByCondition(x => x.EventId == eventId).CountAsync();

                MemberEventView view = new MemberEventView
                {
                    Description = @event.Description,
                    EndRegistration = @event.EndRegistration,
                    EndEvent = @event.EndEvent,
                    SumPrice = @event.SumPrice,
                    Preference = eventPreferences.Preference,
                    MembersCount = memberAttendCount,
                    Reshuffle = @event.Reshuffle
                };

                return Ok(view);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetEventInfo action: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        /// <summary>
        /// Возвращает данные пользователя, необходимые для участия в игре.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="eventId"></param>
        /// <returns>ФИО, адрес, предпочтения</returns>
        [HttpGet("{userId}/preferences/{eventId}")]
        public async Task<ActionResult<PreferencesView>> GetPreferences(Guid userId, Guid eventId)
        {
            try
            {
                var member = await _repository.Member.GetMemberByIdAsync(userId);
                if (member is null)
                {
                    _logger.LogError("Member object is null.");
                    return BadRequest(new { message = "Member not found" });
                }
                var address = await _repository.Address.FindByCondition(x => x.MemberId == userId).FirstOrDefaultAsync();
                var preferences = await _repository.MemberEvent.FindByCondition(x => x.MemberId == userId && x.EventId == eventId).FirstOrDefaultAsync();

                PreferencesView wishes = new PreferencesView
                {
                    Name = member.Surname + " " + member.Name + " " + member.Patronymic,
                    PhoneNumber = address != null ? address.PhoneNumber : null,
                    Zip = address != null ? address.Zip : null,
                    Region = address != null ? address.Region : null,
                    City = address != null ? address.City : null,
                    Street = address != null ? address.Street : null,
                    Apartment = address != null ? address.Apartment : null,
                    Preference = preferences != null ? preferences.Preference : null
                };

                if (preferences is null)
                {
                    return Ok(new { wishes, message = "Member does not participate in the event until now" });
                }
                return Ok(wishes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetWishes action: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        
        /// <summary>
        /// Запись данных пользователя, необходимы для участия в игре.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="eventId"></param>
        /// <param name="preferences"></param>
        /// <returns></returns>
        [HttpPost("{userId}/preferences/{eventId}")]
        public async Task<IActionResult> SendPreferences(Guid userId, Guid eventId, [FromBody] PreferencesPost preferences)
        {
            try
            {
                if (preferences is null)
                {
                    _logger.LogError("Preferences object recived from client is null.");
                    return BadRequest(new { message = "Null object" });
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Preferences object recived from client is not valid.");
                    return BadRequest(new { message = "Invalid object"});
                }
                var @event = await _repository.Event.FindByCondition(x => x.Id == eventId).FirstOrDefaultAsync();
                if (@event is null)
                {
                    _logger.LogError("Event doesn't exist.");
                    return BadRequest(new { message = "Event doesn't exist" });
                }
                if (@event.Reshuffle == true)
                {
                    _logger.LogError("Registration date has already expired");
                    return BadRequest(new { message = "Registration date has already expired" });
                }

                Member member = await _repository.Member.GetMemberByIdAsync(userId);
                var addressSearch = await _repository.Address.FindByCondition(x => x.MemberId == userId).FirstOrDefaultAsync();

                string[] words = preferences.Name.Split(' ');
                member.Surname = words[0];
                member.Name = words[1];
                member.Patronymic = words[2];


                if (addressSearch is null)
                {
                    Address address = new Address
                    {
                        Id = Guid.NewGuid(),
                        MemberId = member.Id,
                        PhoneNumber = preferences.PhoneNumber,
                        Zip = preferences.Zip,
                        Region = preferences.Region,
                        City = preferences.City,
                        Street = preferences.Street,
                        Apartment = preferences.Apartment
                    };
                    _repository.Address.CreateAddress(address);
                }
                else
                {
                    addressSearch.PhoneNumber = preferences.PhoneNumber != null ? preferences.PhoneNumber : addressSearch.PhoneNumber;
                    addressSearch.Zip = preferences.Zip != null ? preferences.Zip : addressSearch.Zip;
                    addressSearch.Region = preferences.Region != null ? preferences.Region : addressSearch.Region;
                    addressSearch.City = preferences.City != null ? preferences.City : addressSearch.City;
                    addressSearch.Street = preferences.Street != null ? preferences.Street : addressSearch.Street;
                    addressSearch.Apartment = preferences.Apartment != null ? preferences.Apartment : addressSearch.Apartment;
                    _repository.Address.UpdateAddress(addressSearch);
                }

                MemberEvent memberEvent = new MemberEvent
                {
                    Id = Guid.NewGuid(),
                    MemberId = userId,
                    EventId = eventId,
                    MemberAttend = true,
                    Preference = preferences.Preference
                };

                _repository.Member.UpdateMember(member);
                _repository.MemberEvent.CreateMemberEvent(memberEvent);
                _repository.MemberEvent.CreateMemberEvent(memberEvent);
                await _repository.SaveAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside SendWishes action: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        
        /// <summary>
        /// Редактировать данные пользователя, необходимые для участия в игре.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="eventId"></param>
        /// <param name="preferences"></param>
        /// <returns></returns>
        [HttpPut("{userId}/preferences/{eventId}")]
        public async Task<IActionResult> UpdatePreferences(Guid userId, Guid eventId, [FromBody] PreferencesPost preferences)
        {
            try
            {
                if (preferences is null)
                {
                    _logger.LogError("Wishes object recived from client is null.");
                    return BadRequest(new { message = "Null object" });
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Wishes object recived from client is not valid.");
                    return BadRequest(new { message = "Invalid object" });
                }
                var @event = await _repository.Event.FindByCondition(x => x.Id == eventId).FirstOrDefaultAsync();
                if (@event is null)
                {
                    _logger.LogError("Event doesn't exist.");
                    return BadRequest(new { message = "Event doesn't exist" });
                }
                if (@event.Reshuffle == true)
                {
                    _logger.LogError("Registration date has already expired");
                    return BadRequest(new { message = "Registration date has already expired" });
                }

                Member member = await _repository.Member.GetMemberByIdAsync(userId);
                var address = await _repository.Address.FindByCondition(x => x.MemberId == userId).FirstOrDefaultAsync();
                var memberEvent = await _repository.MemberEvent.FindByCondition(x => x.MemberId == userId && x.EventId == eventId).FirstOrDefaultAsync();
                if (memberEvent is null)
                {
                    _logger.LogError("Member does not participate in the event");
                    return BadRequest(new { message = "Member does not participate in the event" });
                }

                string[] words = preferences.Name.Split(' ');
                member.Surname = words[0];
                member.Name = words[1];
                member.Patronymic = words[2];

                address.PhoneNumber = preferences.PhoneNumber;
                address.Zip = preferences.Zip;
                address.Region = preferences.Region;
                address.City = preferences.City;
                address.Street = preferences.Street;
                address.Apartment = preferences.Apartment;

                memberEvent.Preference = preferences.Preference;

                _repository.Member.UpdateMember(member);
                _repository.Address.UpdateAddress(address);
                _repository.MemberEvent.UpdateMemberEvent(memberEvent);
                await _repository.SaveAsync();

                //return NoContent();
                return StatusCode(200, "{}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside UpdatePreferences action: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Отказаться от участия в игре.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpPut("{userId}/exit/{eventId}")]
        public async Task<IActionResult> MemberLeaveEvent(Guid userId, Guid eventId)
        {
            try
            {
                var @event = await _repository.Event.FindByCondition(x => x.Id == eventId).FirstOrDefaultAsync();
                if (@event is null)
                {
                    _logger.LogError($"Event doesn't exist");
                    return BadRequest(new { message = "Event doesn't exist" });
                }
                if (@event.Reshuffle == true)
                {
                    _logger.LogError("Registration date has already expired");
                    return BadRequest(new { message = "Registration date has already expired" });
                }
                var member = await _repository.MemberEvent.FindByCondition(x => x.MemberId == userId && x.EventId == eventId).FirstOrDefaultAsync();
                if (member is null || member.MemberAttend is false)
                {
                    _logger.LogError($"Member object not found");
                    return BadRequest(new { message = "Member does not participate in the event" });
                }

                member.MemberAttend = false;
                _repository.MemberEvent.UpdateMemberEvent(member);
                await _repository.SaveAsync();

                //return NoContent();
                return StatusCode(200, "{}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside MemberLeaveEvent action: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Получить данные о получателе подарка.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [HttpGet("{userId}/event/{eventId}/recipientInfo")]
        public async Task<ActionResult<GiftFromMe>> GetPlaceOfDelivery(Guid userId, Guid eventId)
        {
            try
            {
                var recipientId = await _repository.MemberEvent.FindByCondition(x => x.MemberId == userId && x.EventId == eventId).Select(x => x.Recipient).FirstOrDefaultAsync();

                if (recipientId is null)
                {
                    _logger.LogError("Mamber object has not recipient Id.");
                    return BadRequest(new { message = "No recipient Id" });
                }
                else
                {
                    Member recipient = await _repository.Member.GetMemberByIdAsync((Guid)recipientId);
                    var preferences = await _repository.MemberEvent.FindByCondition(x => x.MemberId == (Guid)recipientId && x.EventId == eventId).Select(x => x.Preference).FirstOrDefaultAsync();
                    Address recipientAddress = await _repository.Address.FindByCondition(x => x.MemberId == (Guid)recipientId).FirstAsync();

                    if (recipientAddress.Apartment is null)
                    {
                        GiftFromMe giftFromMe = new GiftFromMe
                        {
                            Name = recipient.Surname + " " + recipient.Name + " " + recipient.Surname,
                            Preferences = preferences != null ? preferences : null,
                            Address = recipientAddress.Zip + ", " + recipientAddress.Region + ", " + recipientAddress.City + ", " + recipientAddress.Street + ", тел. " + recipientAddress.PhoneNumber
                        };
                        return Ok(giftFromMe);
                    }
                    else
                    {
                        GiftFromMe giftFromMe = new GiftFromMe
                        {
                            Name = recipient.Surname + " " + recipient.Name + " " + recipient.Surname,
                            Preferences = preferences != null ? preferences : null,
                            Address = recipientAddress.Zip + ", " + recipientAddress.Region + ", " + recipientAddress.City + ", " + recipientAddress.Street + ", кв. " + recipientAddress.Apartment + ", тел. " + recipientAddress.PhoneNumber
                        };
                        return Ok(giftFromMe);
                    }
                }               
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetPlaceOfDelivery action: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Получить общие данные о пользователе.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{userId}")]
        public async Task<ActionResult<MemberView>> GetMemberById(Guid userId)
        {
            try 
            {            
                var member = await _repository.Member.GetMemberByIdAsync(userId);
                if (member is null)
                {
                    _logger.LogError("Member object not found.");
                    return BadRequest(new { message = "Member not found" });
                }

                MemberView memberView = new MemberView
                {
                    Surname = member.Surname,
                    Name = member.Name,
                    Patronymic = member.Patronymic,
                    Email = member.Email
                };
                return Ok(memberView); 
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong inside GetMemberById action: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }

        }
    }
}
