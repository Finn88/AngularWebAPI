namespace API.Extentions
{
    public static class DateOnlyExtensions
    {
        public static int CalculateAge(this DateOnly dateOnly) {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - dateOnly.Year;
            if (dateOnly > today) age--;
            return age;
        }
    }
}
