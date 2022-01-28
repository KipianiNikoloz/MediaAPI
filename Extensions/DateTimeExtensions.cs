using System;

namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static int CalculateAge(this DateTime DoB)
        {
            var today = DateTime.Today;
            var age = today.Year - DoB.Year;
            if (DoB.Date < today.AddYears(age)) age--;

            return age;
        }
        
        public static DateTime? SetKindUtc(this DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return dateTime.Value.SetKindUtc();
            }
            else
            {
                return null;
            }
        }
        public static DateTime SetKindUtc(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc) { return dateTime; }
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }
    }
}