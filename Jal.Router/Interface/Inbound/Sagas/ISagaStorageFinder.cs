﻿using System;
using Jal.Router.Model.Inbound;

namespace Jal.Router.Interface.Inbound.Sagas
{
    public interface ISagaStorageFinder
    {
        SagaEntity[] GetSagas(DateTime start, DateTime end, string saganame, string sagastoragename = "");
        MessageEntity[] GetMessagesBySaga(SagaEntity sagaentity, string messagestoragename = "");
        MessageEntity[] GetMessages(DateTime start, DateTime end, string routename, string messagestoragename = "");
    }
}