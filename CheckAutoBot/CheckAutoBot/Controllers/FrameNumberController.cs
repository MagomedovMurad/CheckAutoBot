using CheckAutoBot.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CheckAutoBot.Controllers
{
    public interface IFrameNumberController
    {
        Task StartGeneralInfoSearch(string frameNumber, int requestObjectId);
    }

    public class FrameNumberController: IFrameNumberController
    {
        private IMessagesSenderController _messagesSenderController;
        private DbQueryExecutor _queryExecutor;


        public FrameNumberController(IMessagesSenderController messagesSenderController, DbQueryExecutor queryExecutor)
        {
            _messagesSenderController = messagesSenderController;
            _queryExecutor = queryExecutor;
        }

        public async Task StartGeneralInfoSearch(string frameNumber, int requestObjectId)
        {
            var requestObject = await _queryExecutor.GetUserRequestObject(requestObjectId);

            var error = $"😕 К сожалению, поиск информации по автомобилям без VIN кода не выполняется.{Environment.NewLine}" +
                        $"💡 Данная функция находится в разработке.";

            await _messagesSenderController.SendMessage(requestObject.UserId, error);
        }
    }
}
