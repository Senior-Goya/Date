using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository repo;
        private readonly IMapper mapper;
          private readonly IOptions<CloudinaySettings> cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper,
      
        IOptions<CloudinaySettings> cloudinaryConfig)
        {
            this.cloudinaryConfig = cloudinaryConfig;
            this.mapper = mapper;
            this.repo = repo;
            Account acc = new Account(
                this.cloudinaryConfig.Value.CloudName,
                this.cloudinaryConfig.Value.ApiKey,
                this.cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);

        }

    [HttpGet("{id}", Name = "GetPhoto")]
    public async Task<IActionResult> GetPhoto(int id){
        var photoFromRepo = await this.repo.GetPhoto(id);

        var photo = this.mapper.Map<PhotoForReturnDto>(photoFromRepo);
        return Ok(photo);
    }



    [HttpPost]
    public async Task<IActionResult> AddPhotoForUser(int userId, 
    [FromForm]PhotoForCreationDto photoForCreationDto)
        {
         if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var userFromRepo = await this.repo.GetUser(userId);

            var file = photoForCreationDto.File;

            var upLoadResult = new ImageUploadResult();

            if(file.Length > 0){
                using(var stream = file.OpenReadStream()){
                    var uploadParams = new ImageUploadParams(){
                        File  = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    upLoadResult = this._cloudinary.Upload(uploadParams);

                }
            }

            photoForCreationDto.URl = upLoadResult.Url.ToString();
            photoForCreationDto.PublicID = upLoadResult.PublicId;


            var photo = this.mapper.Map<Photo>(photoForCreationDto);

            if(!userFromRepo.Photos.Any(u => u.isMain)){
                photo.isMain = true;

            }

            userFromRepo.Photos.Add(photo);


            if(await this.repo.SaveAll()){
                var photoToReturn = this.mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto",new {userId = userId, id = photo.Id},
                photoToReturn);
            }

            return BadRequest("Could not add the photo");

            
        
        }


        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id) 
        {
             if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await this.repo.GetUser(userId);

            if(!user.Photos.Any( p => p.Id == id)){
                return Unauthorized();
            }

            var photoFromRepo = await this.repo.GetPhoto(id);

            if(photoFromRepo.isMain){
                 return BadRequest("This is already the main photo");
            }

            var currentMainPhoto = await this.repo.GetMainPhotoForUser(userId);
            currentMainPhoto.isMain = false;

            photoFromRepo.isMain = true;

            if(await this.repo.SaveAll()){
                return NoContent();
            }
            return BadRequest("Could not set photo to main");



        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id){
              if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await this.repo.GetUser(userId);

            if(!user.Photos.Any( p => p.Id == id)){
                return Unauthorized();
            }

            var photoFromRepo = await this.repo.GetPhoto(id);

            if(photoFromRepo.isMain){
                 return BadRequest("You cannot delete the Main photo");
            }

            if(photoFromRepo.PublicID != null)
            {
                var deleteParams = new DeletionParams(photoFromRepo.PublicID);
                var result = this._cloudinary.Destroy(deleteParams);

                if(result.Result == "ok")  
                {
                    this.repo.Delete(photoFromRepo);
                }

            }

            if(photoFromRepo.PublicID == null)
            {
                 this.repo.Delete(photoFromRepo);

            }

           

            if(await this.repo.SaveAll()){
                return Ok();
            }
            return BadRequest("Failed to delete the photo");



        }



    }
}

