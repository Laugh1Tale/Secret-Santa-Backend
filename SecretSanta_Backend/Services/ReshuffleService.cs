using Microsoft.EntityFrameworkCore;
using SecretSanta_Backend.Repositories;

namespace SecretSanta_Backend.Services
{
    public class ReshuffleService
    {
        private RepositoryWrapper repository;
        private List<Guid> participants = new List<Guid>();
        private Dictionary<Guid, Guid> assignedPairs = new Dictionary<Guid, Guid>();


        public ReshuffleService()
        {
            this.repository = new RepositoryWrapper();
        }


        public async Task Reshuffle(Guid eventId)
        {
            var @event = await repository.Event.FindAll().Where(x => x.Id == eventId).SingleAsync();
            participants = await repository.MemberEvent.FindAll()
                .Where(x => x.EventId == eventId && x.MemberAttend == true)
                .Select(x => x.MemberId)
                .ToListAsync();
            Shuffle();
            if (participants.Count < 2)
                return;
            else if (@event.SendFriends == true)
                MakePairsWithPairs();
            else
                MakePairsWithoutPairs();
            await SaveReshuffle();
            @event.Reshuffle = true;
            repository.Event.Update(@event);
            await repository.SaveAsync();
        }


        private void Shuffle()
        {
            Random rand = new Random();           
            for (int i = participants.Count - 1; i >= 1; i--)
            {
                int j = rand.Next(i + 1);

                Guid tmp = participants[j];
                participants[j] = participants[i];
                participants[i] = tmp;
            }
        }


        private void MakePairsWithPairs()
        {
            var copyParticipants = new List<Guid>(participants);
            Random rnd = new Random();
            while (participants.Count != 0)
            {
                if (participants.Count == 2)
                {
                    if (participants[0] != copyParticipants[0] && participants[1] != copyParticipants[1])
                    {
                        assignedPairs.Add(participants[0], copyParticipants[0]);
                        assignedPairs.Add(participants[1], copyParticipants[1]);
                    }
                    else
                        MakePairsWithoutPairs();
                    break;
                }
                var randomIndex = rnd.Next(copyParticipants.Count - 1);
                assignedPairs.Add(participants[0], copyParticipants.Where(x => x != participants[0]).ToList()[randomIndex]);
                copyParticipants = new List<Guid>(copyParticipants.Where(x => x != copyParticipants.Where(x => x != participants[0]).ToList()[randomIndex]));
                participants.RemoveAt(0);
            }
        }


        private void MakePairsWithoutPairs()
        {
            assignedPairs = participants.Where((e, i) => i < participants.Count - 1)
                .Select((e, i) => new { A = e, B = participants[i + 1] }).ToDictionary(x=> x.A, x=> x.B);
            assignedPairs.Add(participants[participants.Count - 1], participants[0]);
        }


        private async Task SaveReshuffle()
        {
            foreach (var assignedPair in assignedPairs)
            {
                var participant = await repository.MemberEvent.FindByCondition(x => x.MemberId == assignedPair.Key).SingleAsync();
                participant.Recipient = assignedPair.Key;
                repository.MemberEvent.Update(participant);
            }
            await repository.SaveAsync();
        }
    }
}
