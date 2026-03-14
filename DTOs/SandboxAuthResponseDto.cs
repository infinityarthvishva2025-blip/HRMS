namespace HRMS.DTOs
{
    
    public class SandboxAuthResponseDto
    {
        public SandboxAuthData data { get; set; }
    }

    public class SandboxAuthData
    {
        public string access_token { get; set; }
    }
}
