using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SecretSanta_Backend.Interfaces;
using SecretSanta_Backend.Repositories;
using System.Net;
using System.Net.Mail;

namespace SecretSanta_Backend.Services
{
    public class MailService
    {
        private RepositoryWrapper repository;
        private IConfiguration config;

        public MailService()
        {
            config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();
            repository = new RepositoryWrapper();
        }

        private async Task SendMail(MailMessage mailMessage)
        {
            try
            {
                using (var message = mailMessage)
                {
                    using (var client = InitializeSmtpClient())
                    {
                        await client.SendMailAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        
        private SmtpClient InitializeSmtpClient() => new SmtpClient
        {
            EnableSsl = config.GetValue<bool>("EmailSettings:EnableSsl"),
            Host = config.GetValue<string>("EmailSettings:Host"),
            Port = config.GetValue<int>("EmailSettings:Port"),
            Credentials = new NetworkCredential(config.GetValue<string>("EmailSettings:From"),
            config.GetValue<string>("EmailSettings:Password"))
        };


        private async Task<MailMessage> CreateDesignatedRecipientMessage(string email, Guid eventId, Guid memberId)
        {
            var recommendedSum = await repository.Event.FindByCondition(x => x.Id == eventId).Select(x => x.SumPrice).SingleAsync();
            var endOfEvent = await repository.MemberEvent.FindByCondition(x => x.MemberId == memberId).Select(x => x.SendDay).SingleAsync();
            var recipientId = await repository.MemberEvent.FindByCondition(x => x.MemberId == memberId && x.EventId == eventId).Select(x => x.Recipient).SingleAsync();
            var recipient = await repository.Member.FindByCondition(x => x.Id == recipientId).SingleAsync();
            var recipientInEvent = await repository.MemberEvent.FindByCondition(x => x.MemberId == recipientId && x.EventId == eventId).SingleAsync();
            var preference = recipientInEvent.Preference;
            var address = await repository.Address.FindByCondition(x => x.MemberId == recipientId).SingleAsync();
            MailMessage message = new MailMessage("secret-santa-test@mail.ru", email);
            message.Subject = "Распределение получателей подарков в игре Secret Santa подошло к концу.";
            message.Body = CreateDesignatedRecipientMailBody(recipient, preference, address, endOfEvent, recommendedSum);
            return message;
        }


        private string CreateDesignatedRecipientMailBody(Models.Member recipient, string? preference, Models.Address address, DateTime? endOfEvent, int? recommendedSum) =>
            string.Format(@"Срок регистрации подошёл к концу, а значит вы теперь - тайный санта. " +
                "Получатель вашего подарка - {1} {2} {3}.{0}" +
                "{0}" +
                (preference is not null ? "Ваш получатель высказал свои пожелания по подарку, вот они: \"{4}\".{0}" :
                "У вашего получателя нет никаких пожеланий насчёт подарка, постарайтесь его приятно удивить!{0}") +
                "{0}" +
                "Вот адрес вашего получателя: {0}" +
                "      Регион: {5}.{0}" +
                "      Город: {6}.{0}" +
                "      Улица, дом: {7}.{0}" +
                "      Квартира: {8}.{0}" +
                "      Индекс: {9}.{0}" +
                "{0}" +
                "И номер телефона для отправки посылки: +7{10}.{0}" +
                "{0}" +
                 (recommendedSum is not null ? "Рекомендуемая стоимость подарка: {11} рублей.{0}" : "") +
                "Отправьте подарок не позднее, чем {12}.{0}" +
                "{0}" +
                "Надеемся вы сможете порадовать своего получателя!", Environment.NewLine, recipient.Surname, recipient.Name, recipient.Patronymic,
                preference, address.Region, address.City, address.Street,  address.Apartment, address.Zip, address.PhoneNumber, recommendedSum, endOfEvent);



        private async Task<MailMessage> CreateDateChangesMessage(string email, Guid eventId)
        {
            var game = await repository.Event.FindByCondition(x => x.Id == eventId).SingleAsync();
            var endOfRegistration = game.EndRegistration;
            var EndOfEvent = game.EndEvent;
            MailMessage message = new MailMessage("secret-santa-test@mail.ru", email);
            message.Subject = "Изменение сроков проведения игры Secret Santa.";
            message.Body = String.Format("Приносим извинения за перенос сроков проведения события.{0}" +
                "{0}" +
                "Регистрация и назначение получателя вашего подарка произойдет: {1},{0}" +
                "Конец события назначен на: {2}", Environment.NewLine, endOfRegistration, EndOfEvent);
            return message;
        }


        public async Task sendEmailsWithDesignatedRecipient(Guid eventId)
        {
            var memberIds = await repository.MemberEvent
                .FindByCondition(x => x.EventId == eventId && x.MemberAttend == true).Select(x => x.MemberId).ToListAsync();
            var members = await repository.Member.FindByCondition(x => memberIds.Contains(x.Id)).ToListAsync();         
            foreach (var member in members)
            {
                var email = member.Email;
                var memberId = member.Id;
                var message = await CreateDesignatedRecipientMessage(email, eventId, memberId);
                await SendMail(message);
            }
        }


        public async Task SendEmailsWithDateChanges(Guid eventId)
        {
            var memberIds = await repository.MemberEvent
                .FindByCondition(x => x.EventId == eventId && x.MemberAttend == true).Select(x => x.MemberId).ToListAsync();
            var emails = await repository.Member.FindByCondition(x => memberIds.Contains(x.Id)).Select(x => x.Email).ToListAsync();
            foreach (var email in emails)
            {
                var message = await CreateDateChangesMessage(email, eventId);
                await SendMail(message);
            }
        }
    }
}
