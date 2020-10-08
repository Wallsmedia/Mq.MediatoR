// Copyright © Alexander Paskhin 2019. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System.Collections.Generic;

namespace Mq.MediatoR.Abstractions.Test.NotificationHandlers.Mock
{
    class TestPublishNotication
    {
        public string Text { get; set; } = nameof(TestPublishNotication);
        public List<string> Visitor { get; } = new List<string>();
    }

}
