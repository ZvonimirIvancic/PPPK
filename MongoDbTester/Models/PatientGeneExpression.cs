using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDbTester.Models
{
    public class PatientGeneExpression
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("patient_id")]
        public string PatientId { get; set; } = string.Empty;

        [BsonElement("cancer_cohort")]
        public string CancerCohort { get; set; } = "SARC";

        [BsonElement("gene_expressions")]
        public Dictionary<string, double> GeneExpressions { get; set; } = new();

        [BsonElement("cgas_sting_genes")]
        public cGAS_STING_GenesExpression? cGAS_STING_Genes { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
