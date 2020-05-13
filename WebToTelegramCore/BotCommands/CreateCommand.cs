﻿using System;
using WebToTelegramCore.Models;
using WebToTelegramCore.Options;
using WebToTelegramCore.Services;

namespace WebToTelegramCore.BotCommands
{
    /// <summary>
    /// /create command, allows user to create a token and start using the bot.
    /// </summary>
    public class CreateCommand : GuestOnlyCommandBase, IBotCommand
    {
        /// <summary>
        /// Command's text.
        /// </summary>
        public override string Command => "/create";

        /// <summary>
        /// Message to display on token creation. Must be formatted, {0} is token.
        /// </summary>
        private readonly string _message;

        /// <summary>
        /// Message to display when registration is off.
        /// </summary>
        private readonly string _goAway;

        /// <summary>
        /// Field to store database context reference.
        /// </summary>
        private readonly RecordContext _context;

        /// <summary>
        /// Field to store token generator reference.
        /// </summary>
        private readonly ITokenGeneratorService _generator;

        /// <summary>
        /// Field to store whether registration is enabled. True is enabled.
        /// </summary>
        private readonly bool _isRegistrationEnabled;

        /// <summary>
        /// Constructor that injects dependencies and sets up registration state.
        /// </summary>
        /// <param name="locale">Locale options to use.</param>
        /// <param name="context">Database context to use.</param>
        /// <param name="generator">Token generator service to use.</param>
        /// <param name="isRegistrationEnabled">State of registration.</param>
        public CreateCommand(LocalizationOptions locale, RecordContext context,
            ITokenGeneratorService generator, bool isRegistrationEnabled) : base(locale)
        {
            _context = context;
            _generator = generator;
            _isRegistrationEnabled = isRegistrationEnabled;

            _message = locale.CreateSuccess;
            _goAway = locale.CreateGoAway;
        }

        /// <summary>
        /// Method to process the command.
        /// </summary>
        /// <param name="record">Record to process.</param>
        /// <returns>Message with new token or error when there is one already.</returns>
        public override string Process(long userId, Record record)
        {
            return base.Process(userId, record) ?? InternalProcess(userId, record);
        }

        /// <summary>
        /// Actual method that does registration or denies it.
        /// </summary>
        /// <param name="userId">Telegram user ID to create a new record. _The_ place in the app
        /// it's used.</param>
        /// <param name="record">Record to process. Is null if working properly.</param>
        /// <returns>Message with new token or message stating that registration
        /// is closed for good.</returns>
        private string InternalProcess(long userId, Record record)
        {
            if (_isRegistrationEnabled)
            {
                string token = _generator.Generate();
                Record r = new Record() { AccountNumber = userId, Token = token };
                _context.Add(r);
                _context.SaveChanges();
                return String.Format(_message, token);
            }
            else
            {
                return _goAway;
            }
        }
    }
}
