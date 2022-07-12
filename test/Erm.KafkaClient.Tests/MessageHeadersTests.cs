using System.Text;
using Confluent.Kafka;
using FluentAssertions;
using Xunit;

namespace Erm.KafkaClient.Tests;

public class MessageHeadersTests
{
    private const string Key = "abc";
    private const string StrValue = "123";
    private readonly byte[] _value = Encoding.UTF8.GetBytes("123");

    [Fact]
    public void Add_WithKeyNotNull_ShouldAddValueCorrectly()
    {
        var header = new KafkaMessageHeaders
        {
            { Key, _value }
        };

        header[Key].Should().BeEquivalentTo(_value);
    }

    [Fact]
    public void Add_WithValueNull_ShouldAddValueCorrectly()
    {
        var header = new KafkaMessageHeaders
        {
            { Key, null }
        };

        header[Key].Should().BeNull();
    }

    [Fact]
    public void GetKafkaHeader_ShouldReturnKafkaHeaders()
    {
        var kafkaHeaders = new Headers { { Key, _value } };
        var messageHeaders = new KafkaMessageHeaders(kafkaHeaders);
        var result = messageHeaders.GetKafkaHeaders();
        result.Should().BeEquivalentTo(kafkaHeaders);
    }

    [Fact]
    public void SetString_WithValueNotNull_ShouldAddValueCorrectly()
    {
        var header = new KafkaMessageHeaders();
        header.AddString(Key, StrValue);
        header.GetString(Key).Should().BeEquivalentTo(StrValue);
    }
}