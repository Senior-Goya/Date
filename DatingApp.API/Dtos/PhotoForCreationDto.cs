using System;
using Microsoft.AspNetCore.Http;

namespace DatingApp.API.Dtos
{
    public class PhotoForCreationDto
    {
        public string URl { get; set; }
        public IFormFile File { get; set; }
        public string Description { get; set; } 
        public DateTime DateAdded { get; set; }
        public string PublicID { get; set; }
        
    }
}