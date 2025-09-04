using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDbTester.Models
{
    public class cGAS_STING_GenesExpression
    {
        [BsonElement("c6orf150_cgas")]
        public double C6orf150_cGAS { get; set; }

        [BsonElement("ccl5")]
        public double CCL5 { get; set; }

        [BsonElement("cxcl10")]
        public double CXCL10 { get; set; }

        [BsonElement("tmem173_sting")]
        public double TMEM173_STING { get; set; }

        [BsonElement("cxcl9")]
        public double CXCL9 { get; set; }

        [BsonElement("cxcl11")]
        public double CXCL11 { get; set; }

        [BsonElement("nfkb1")]
        public double NFKB1 { get; set; }

        [BsonElement("ikbke")]
        public double IKBKE { get; set; }

        [BsonElement("irf3")]
        public double IRF3 { get; set; }

        [BsonElement("trex1")]
        public double TREX1 { get; set; }

        [BsonElement("atm")]
        public double ATM { get; set; }

        [BsonElement("il6")]
        public double IL6 { get; set; }

        [BsonElement("il8_cxcl8")]
        public double IL8_CXCL8 { get; set; }
    }
}
