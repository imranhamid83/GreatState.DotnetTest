namespace GreatState.DotnetTest.API.Models
{
    public class Page
    {

        public int Id { get; set; }
        public required string Title { get; set; }

        public required string Body { get; set; }
        public required string RoleRequired { get; set; }
    }
}
