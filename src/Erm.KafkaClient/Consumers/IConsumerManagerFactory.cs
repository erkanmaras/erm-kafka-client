using System;
using Erm.KafkaClient.Configuration;

namespace Erm.KafkaClient.Consumers;

internal interface IConsumerManagerFactory
{
    IConsumerManager Create(IConsumerConfiguration configuration, IServiceProvider serviceProvider);
}