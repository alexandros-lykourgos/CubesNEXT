using Cubes.Core.DataAccess;

namespace Cubes.Web.UIHelpers.Schema.CoreProviders
{
    public class QuerySchemaProvider : SchemaProvider<Query>
    {
        public override Schema GetSchema()
            => Schema.Create(this.Name, "Database Query")
               .WithText("name", Validator.Required(), Validator.Pattern(@"^\S+$"))
               .WithText("comments")
               .WithItem
                    .TextArea("queryCommand", 8, Validator.Required())
                    .HasClass("code")
                    .Build();
    }
}
