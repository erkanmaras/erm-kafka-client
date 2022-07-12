using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Erm.KafkaClient.IntegrationTests;

public class KafkaClientTests
{
    [Fact]
    public async Task ConsumedMessageCount_ShouldEqualTo_ProducedMessageCount()
    {
        var messageCount = 10;
        using (var kafkaClient = new KafkaClientScope())
        {
            for (var i = 0; i < messageCount; i++)
            {
                await kafkaClient.Producer.ProduceAsync(kafkaClient.TopicName, Encoding.UTF8.GetBytes(i.ToString()), Encoding.UTF8.GetBytes(i.ToString()));
            }

            await kafkaClient.WaitForConsume(messageCount);
            kafkaClient.ConsumedMessageStore.Messages.Should().HaveCount(messageCount);
        }
    }

    [Fact]
    public async Task Consumer_ShouldRead_AllReadyExistedMessages()
    {
        var topicName = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var messageCount = 10;
        using (var kafkaClient = new KafkaClientScope(topicName, consume: false))
        {
            for (var i = 0; i < messageCount; i++)
            {
                await kafkaClient.Producer.ProduceAsync(kafkaClient.TopicName, Encoding.UTF8.GetBytes(i.ToString()), Encoding.UTF8.GetBytes(i.ToString()));
            }
        }
        
        using (var kafkaClient = new KafkaClientScope(topicName, consume: true))
        {
            await kafkaClient.WaitForConsume(messageCount);
            kafkaClient.ConsumedMessageStore.Messages.Should().HaveCount(messageCount);
        }
    }

    [Fact]
    public async Task ProducedMessages_ShouldConsume_SameOrder()
    {
        var messageCount = 10;
        using (var kafkaClient = new KafkaClientScope())
        {
            for (var i = 0; i < messageCount; i++)
            {
                kafkaClient.Producer.Produce(kafkaClient.TopicName, Encoding.UTF8.GetBytes("key"), Encoding.UTF8.GetBytes(i.ToString()));
            }

            await kafkaClient.WaitForConsume(messageCount);

            kafkaClient.ConsumedMessageStore.Messages.Should().HaveCount(messageCount);
            var message = kafkaClient.ConsumedMessageStore.Messages.Reverse().ToList();
            for (var i = 0; i < messageCount; i++)
            {
                i.ToString().Should().Be(message[i]);
            }
        }
    }
}