namespace RoutineApi.Models
{
    public class LinkDto
    {
        public LinkDto(string herf, string rel, string method)
        {
            Herf = herf;
            Rel = rel;
            Method = method;
        }

        public string Herf { get; set; }
        public string Rel { get; set; }
        public string Method { get; set; }

    }
}
