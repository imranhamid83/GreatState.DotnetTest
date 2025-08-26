using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace GreatState.DotnetTest.API.Models
{
    public class Role
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public required string Key { get; set; }
    }
}
