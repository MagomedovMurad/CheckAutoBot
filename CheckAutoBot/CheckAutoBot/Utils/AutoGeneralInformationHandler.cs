using CheckAutoBot.Handlers;
using CheckAutoBot.Managers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Utils
{
    public class AutoGeneralInformationHandler : GibddHandler
    {
        public AutoGeneralInformationHandler(GibddManager gibddManager, RucaptchaManager rucaptchaManager) 
                                      : base(gibddManager, rucaptchaManager)
        {
        }


    }
}
