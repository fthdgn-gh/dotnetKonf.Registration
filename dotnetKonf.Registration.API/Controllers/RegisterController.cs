using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.SimpleEmail;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using dotnetKonf.Registration.API.Extensions;
using dotnetKonf.Registration.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace dotnetKonf.Registration.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        public IAmazonDynamoDB DB { get; set; }
        public Table Entries { get; set; }
        public IAmazonSimpleNotificationService SNS { get; set; }
        public IAmazonSimpleEmailService Mailer { get; set; }
        public RegisterController(IAmazonDynamoDB db, IAmazonSimpleNotificationService sns, IAmazonSimpleEmailService mailer)
        {
            DB = db;
            Entries = Table.LoadTable(DB, "EntriesDemo");
            SNS = sns;
            Mailer = mailer;
        }
        
        public async Task<ApiResponse<EntryResponseModel>> Post()
        {
            var response = new ApiResponse<EntryResponseModel>();
            var model = Request.Body.Deserialize<EntryModel>();
            if (model == null) return response.AsError("Model is required");
            if (string.IsNullOrEmpty(model.Name)) return response.AsError("Name is required");
            if (string.IsNullOrEmpty(model.Surname)) return response.AsError("Surname is required");
            if (string.IsNullOrEmpty(model.Company)) return response.AsError("Company is required");
            if (string.IsNullOrEmpty(model.Title)) return response.AsError("Title is required");
            if (string.IsNullOrEmpty(model.MailAddress)) return response.AsError("Mail address is required");
            if (string.IsNullOrEmpty(model.PhoneNumber)) return response.AsError("Phone number is required");

            var mailAddress = model.MailAddress;
            var phoneNumber = model.PhoneNumber;
            if (!mailAddress.Contains("@")) return response.AsError("Mail address is not valid");
            if (phoneNumber.Length < 12 || phoneNumber.Length > 15 || !new Regex("[+][0-9]*").Match(phoneNumber).Success) return response.AsError("Phone number is not valid");

            Entry entry = new Entry();
            entry.Name = model.Name;
            entry.Surname = model.Surname;
            entry.Company = model.Company;
            entry.Title = model.Title;
            entry.MailAddress = model.MailAddress;
            entry.PhoneNumber = model.PhoneNumber;
            entry.IsVerified = false;
            entry.VerifyCode = Guid.NewGuid().ToString().Substring(0, 6);

            // TODO: Insert item to table.
            await Entries.PutItemAsync(Document.FromJson(JsonConvert.SerializeObject(entry)));
            // TODO: Notify the entry
            await NotifyEntry(entry);
            return response.AsSuccess(new EntryResponseModel { Id = entry.Id }, "Your code will be sent to you via mail and sms.");
        }


        [Route("{id}")]
        public async Task<ApiResponse<object>> Put(string id)
        {
            var response = new ApiResponse<object>();
            var model = Request.Body.Deserialize<EntryVerifyModel>();
            if (model == null) return response.AsError("Model is required");
            if (string.IsNullOrEmpty(model.Action)) return response.AsError("Action is required");

            // TODO: Get document
            Document document = await Entries.GetItemAsync(id);
            if (document == null) return response.AsError("Entry not found");
            var entry = JsonConvert.DeserializeObject<Entry>(document.ToJson());
            if (entry.IsVerified) return response.AsError("Entry is already verified");
            switch (model.Action.ToLower())
            {
                case "verify":
                    if (string.IsNullOrEmpty(model.VerifyCode)) return response.AsError("Verify code is required");
                    if (entry.VerifyCode == model.VerifyCode)
                    {
                        document[nameof(Entry.IsVerified)] = true;
                        // TODO: Update entry
                        await Entries.UpdateItemAsync(document);
                        return response.AsSuccess(null, "Entry verified successfully");
                    }
                    return response.AsError("Verify codes is not the same. You can reset and receive another one.");
                case "reset":
                    entry.VerifyCode = Guid.NewGuid().ToString().Substring(0, 6);
                    document[nameof(Entry.VerifyCode)] = entry.VerifyCode;
                    // TODO: Update entry
                    await Entries.UpdateItemAsync(document);
                    // TODO: Notify entry
                    await NotifyEntry(entry);
                    return response.AsSuccess(null, "You will receive another message");
            }

            return response.AsError("Unknown action");
        }

        protected async Task NotifyEntry(Entry entry)
        {
            try
            {
                await SNS.PublishAsync(new PublishRequest { PhoneNumber = entry.PhoneNumber, Message = $"Your verification code is '{entry.VerifyCode}'" });
            }
            catch (Exception e) {}

            try
            {
                await Mailer.SendEmailAsync(new Amazon.SimpleEmail.Model.SendEmailRequest
                {
                    Source = "fth.dgn@outlook.com",
                    Destination = new Amazon.SimpleEmail.Model.Destination
                    {
                        ToAddresses = new List<string> {
                            entry.MailAddress
                        }
                    },
                    Message = new Amazon.SimpleEmail.Model.Message
                    {
                        Subject = new Amazon.SimpleEmail.Model.Content("dotnetKonf Registration"),
                        Body = new Amazon.SimpleEmail.Model.Body
                        {
                            Html = new Amazon.SimpleEmail.Model.Content($"Your verification code is '{entry.VerifyCode}'"),
                            Text = new Amazon.SimpleEmail.Model.Content($"Your verification code is '{entry.VerifyCode}'")
                        }
                    }
                });
            }
            catch (Exception e) {}
        }
    }
}