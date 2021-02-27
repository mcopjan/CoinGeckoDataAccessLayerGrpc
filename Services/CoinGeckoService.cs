using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GreeterGrpc.Services
{
    public class CoinGeckoService : ExchangeService.ExchangeServiceBase
    {
        private const string username = "root";
        private const string password = "example";

        private readonly ILogger<CoinGeckoService> _logger;
        //mongodb://username:password@myserver/databaseName
        private static MongoClient mongoCLient = new MongoClient($"mongodb://{username}:{password}@localhost:27017");
        private static IMongoDatabase mongoDatabase = mongoCLient.GetDatabase("mydb");
        private static IMongoCollection<BsonDocument> mongoCollection = mongoDatabase.GetCollection<BsonDocument>("exchange");

        public CoinGeckoService(ILogger<CoinGeckoService> logger)
        {
            _logger = logger;
        }

        public override Task<StoreExchangeResponse> StoreExchange(StoreExchangeRequest request, ServerCallContext context)
        {
            var exchange = request.Exchange;
            BsonDocument bson = new BsonDocument("country", exchange.Country)
                .Add("id", exchange.Id)
                .Add("name", exchange.Name)
                .Add("has_trading_incentive", exchange.HasTradingIncentive)
                .Add("image", exchange.Image)
                .Add("trade_volume_24h_btc_normalized", exchange.TradeVolume24HBtcNormalized)
                .Add("trust_score", exchange.TrustScore)
                .Add("score_rank", exchange.TrustScoreRank)
                .Add("url", exchange.Url)
                .Add("year_established", exchange.YearEstablished)
                ;
            mongoCollection.InsertOne(bson);

            string id = bson.GetValue("_id").ToString();
            exchange.MongoId = id;

            return Task.FromResult(new StoreExchangeResponse
            {
                Result = exchange
            }); 
        }
    }
}
