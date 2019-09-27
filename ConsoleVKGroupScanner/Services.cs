using System;
using System.Collections.Generic;
using System.Text;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Model.Attachments;
using System.Net;
using System.IO;

namespace ConsoleVKGroupScanner
{
    public class Services
    {
        // <summary>
        /// Возращает URL самой большой фотографиии из существующих
        /// </summary>
        public string GetUrlOfBigPhoto(VkNet.Model.Attachments.Photo photo)
        {
            if (photo == null)
                return null;
            if (photo.Photo2560 != null)
                return photo.Photo2560.AbsoluteUri;
            if (photo.Photo1280 != null)
                return photo.Photo1280.AbsoluteUri;
            if (photo.Photo807 != null)
                return photo.Photo807.AbsoluteUri;
            if (photo.Photo604 != null)
                return photo.Photo604.AbsoluteUri;
            if (photo.Photo130 != null)
                return photo.Photo130.AbsoluteUri;
            if (photo.Photo75 != null)
                return photo.Photo75.AbsoluteUri;
            if (photo.Sizes?.Count > 0)
            {
                var bigSize = photo.Sizes[0];
                for (int i = 0; i < photo.Sizes.Count; i++)
                {
                    var photoSize = photo.Sizes[i];
                    if (photoSize.Height > bigSize.Height && photoSize.Width > bigSize.Width)
                        bigSize = photoSize;
                }
                return bigSize.Src.AbsoluteUri;
            }
            return null;
        }

        // <summary>
        /// Тянем инфу о коментах
        /// </summary>
        public string GetCommentsOfPost(long? postId, VkApi api, long groupId)
        {
            if (postId != null)
            {
                long postIdLong = (long)postId;
                string resultString = "";
                long cou = 0;
                var comments = api.Wall.GetComments(new WallGetCommentsParams
                {
                    NeedLikes = false,
                    PostId = postIdLong,
                    OwnerId = groupId,
                    Count = 100,
                    Offset = 0,
                    Extended = true,
                    Sort = VkNet.Enums.SortOrderBy.Asc,
                    PreviewLength = 0
                });
                
                foreach (var comnts in comments.Items)
                {
                    string ownerComment = "https://vk.com/id" + comnts.FromId.ToString();
                    string textCommnet = comnts.Text;
                    resultString = resultString + " CommentOwner: " + ownerComment + " CommentText: " + textCommnet + " ";
                }
                return resultString + comments.Count.ToString();
            }
            return "postId is null";
        }


        public string GetInfoOfPost(long? postId, VkApi api, long groupId, string infoType)
        {
            if (postId != null)
            {
                long postIdLong = (long)postId;
                List<String> posts = new List<string>();
                posts.Add(groupId + "_" + postIdLong.ToString());

                var post = api.Wall.GetById(posts);
                foreach (var psts in post.WallPosts)
                {
                    switch (infoType)
                    {
                        case "likes":
                            return psts.Likes.Count.ToString(); 
                        case "date":
                            return psts.Date.Value.AddHours(2).ToString();
                    } 
                }
            }
            return "postId is null";
        }

        public string GetPhotoLink(VkNet.Model.Attachments.Post post)
        {
            string stringAttachments = "";
            //for(int i = 0; i = post.Attachments.Count; i++)
            //using (WebClient webClient = new WebClient())
            //{
                foreach (var x in post.Attachments)
                {
                    if (x.Type.ToString().Equals("VkNet.Model.Attachments.Photo"))
                    {                   
                        stringAttachments = stringAttachments + "https://vk.com/photo" + "-37583795_" + x.Instance.Id.ToString() + " ";

                        //webClient.DownloadFile("https://vk.com/photo" + "-37583795_" + x.Instance.Id.ToString(), "wallPhotos\\"+x.Instance.Id.ToString()+".jpeg");
                        //Console.WriteLine(x.Instance.Id.ToString());
                    }
                }
            //}
                
            return stringAttachments;
        }

        /*
        public string GetPhotoURI(VkNet.Model.Attachments.Post post)
        {
            string stringURI = "";
            //for(int i = 0; i = post.Attachments.Count; i++)
            foreach (var x in post.Attachments)
            {
                VkNet.Model.Attachments.Photo photoType;
                if (x.Type.GetType.ToString().Equals(photoType.GetType.ToString()))
                {
                    List<VkNet.Model.Attachments.Photo> phList= new List<VkNet.Model.Attachments.Photo>();
                    phList.Add(x.Instance);
                    stringURI = stringURI + x.Instance.Id.ToString() + GetUrlOfBigPhoto(x.Instance);
                }
            }
            return stringURI;
        }
        */
    }
}
