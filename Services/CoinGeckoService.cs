using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Coingecko;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
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
            if (exchange != null)
            {
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
            }
            

            return Task.FromResult(new StoreExchangeResponse
            {
                Result = exchange
            }); 
        }

        public override async Task<GetExchangeResponse> GetExchange(GetExchangeRequest request, ServerCallContext context)
        {
            BsonDocument bson;
            try
            {
                bson = mongoCollection.Find($"{{ _id: ObjectId('{request.Id}') }}").FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Unknown, $"Could get Exchange with id {request.Id} -> {ex.Message}"));
            }

            if (bson ==null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Could not find Exchange with id {request.Id}"));
            }

            var exchange = new Exchange() {
                Country = bson.GetValue("country").AsString,
                Id = bson.GetValue("id").AsString,
                Name = bson.GetValue("name").AsString,
                HasTradingIncentive = bson.GetValue("has_trading_incentive").AsString,
                Image = bson.GetValue("image").AsString,
                TradeVolume24HBtcNormalized = bson.GetValue("trade_volume_24h_btc_normalized").AsString,
                TrustScore = bson.GetValue("trust_score").AsString,
                TrustScoreRank = bson.GetValue("score_rank").AsString,
                Url = bson.GetValue("url").AsString,
                YearEstablished = bson.GetValue("year_established").AsString
            };
            var response = new GetExchangeResponse { Result = exchange };

            return await Task.FromResult(response);
        }


        public override async Task<UpdateExchangeResponse> UpdateExchange(UpdateExchangeRequest request, ServerCallContext context)
        {
            BsonDocument bson;
            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(request.Exchange.MongoId));

            try
            {
                bson = mongoCollection.Find(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Unknown, $"Could get Exchange with id {request.Exchange.MongoId} -> {ex.Message}"));
            }

            if (bson == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Could not find Exchange with id {request.Exchange.MongoId}"));
            }

            BsonDocument updateBson = new BsonDocument("country", request.Exchange.Country)
                .Add("id", request.Exchange.Id)
                .Add("name", request.Exchange.Name)
                .Add("has_trading_incentive", request.Exchange.HasTradingIncentive)
                .Add("image", request.Exchange.Image)
                .Add("trade_volume_24h_btc_normalized", request.Exchange.TradeVolume24HBtcNormalized)
                .Add("trust_score", request.Exchange.TrustScore)
                .Add("score_rank", request.Exchange.TrustScoreRank)
                .Add("url", request.Exchange.Url)
                .Add("year_established", request.Exchange.YearEstablished);

            await mongoCollection.ReplaceOneAsync(filter, updateBson);

            return await Task.FromResult(new UpdateExchangeResponse());
            
                //tbd
            //});

        }

        public override async Task<DeleteExchangeResponse> DeleteExchange(DeleteExchangeRequest request, ServerCallContext context)
        {
            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(request.Id));
            var res = await mongoCollection.DeleteOneAsync(filter);
            return new DeleteExchangeResponse() { IsAcknowledged = res.IsAcknowledged };
        }


    }
}
