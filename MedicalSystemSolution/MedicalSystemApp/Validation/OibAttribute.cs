using System.ComponentModel.DataAnnotations;

namespace MedicalSystemApp.Validation
{
    public class OibAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            var s = value as string;
            if (string.IsNullOrWhiteSpace(s) || s.Length != 11 || !s.All(char.IsDigit)) return false;

            int a = 10;
            for (int i = 0; i < 10; i++)
            {
                a = a + (s[i] - '0'); a %= 10;
                if (a == 0) a = 10;
                a *= 2; a %= 11;
            }
            int kontrolni = 11 - a;
            if (kontrolni == 10) kontrolni = 0;
            return kontrolni == (s[10] - '0');
        }
    }
}
