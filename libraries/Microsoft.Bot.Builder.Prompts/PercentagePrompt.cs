﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{

    /// <summary>
    /// PercentagePrompt recognizes percentage expressions as float type
    /// </summary>
    public class PercentagePrompt : BasePrompt<NumberResult<float>>
    {
        private IModel _model;

        public PercentagePrompt(string culture, PromptValidator<NumberResult<float>> validator = null) 
            : base( validator)
        {
            _model = NumberRecognizer.Instance.GetPercentageModel(culture);
        }


        protected PercentagePrompt(IModel model, PromptValidator<NumberResult<float>> validator = null)
            : base(validator)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// Used to validate the incoming text, expected on context.Request, is
        /// valid according to the rules defined in the validation steps. 
        /// </summary>        
        public override async Task<NumberResult<float>> Recognize(IBotContext context)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Request);
            if (context.Request.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");

            NumberResult<float> numberResult = new NumberResult<float>();
            IMessageActivity message = context.Request.AsMessageActivity();
            var results = _model.Parse(message.Text);
            if (results.Any())
            {
                var result = results.First();
                if (float.TryParse(result.Resolution["value"].ToString().TrimEnd('%'), out float value))
                {
                    numberResult.Status = RecognitionStatus.Recognized;
                    numberResult.Value = value;
                    numberResult.Text = result.Text;
                    await Validate(context, numberResult);
                }
            }
            return numberResult;
        }
    }
}
