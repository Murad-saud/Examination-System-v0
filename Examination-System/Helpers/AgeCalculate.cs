namespace Examination_System.Helpers
{
    public static class AgeCalculate
    {
        public static int GetAge(DateTime dateOfBirth)
        {
            return DateTime.UtcNow.Year - dateOfBirth.Year;
        }
    }
}
