﻿namespace Customs_Management_System.DTOs
{
    public class LoginResponseDto
    {

        public string UserName { get; set; }
        public string Role { get; set; }
        public string Token { get; set; } // JWT token

    }
}
