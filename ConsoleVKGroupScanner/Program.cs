using System;
using CsvHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;


namespace ConsoleVKGroupScanner
{
    class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 10000;
            return w;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var api = new VkApi();
            string linkHead = "https://vk.com/";
            int groupId = -37583795;
            MyWebClient webClient = new MyWebClient();
            System.IO.File.Create("wallPhotoList.csv").Close();
            System.IO.File.Create("albumPhotoList.csv").Close();
            Services fromServices = new Services();
            List<wallPhotoModel> wallPhotoList = new List<wallPhotoModel>();
            List<albumPhotoModel> albumPhotoList = new List<albumPhotoModel>();
            int offsertPostExit;
            int offsetPhotoExit;
            int offsetAlbumExit;
            int offsetWallPhotoSave;
            int offsetAlbumPhotoSave;
            int countPosts = 0;
            int countPhotos = 0;
            int countPhotosWSave = 0;
            int countPhotosASave = 0;

            //summary
            //authorization
            //summary
            api.Authorize(new ApiAuthParams
            {
                ApplicationId = //Token here,
                Login = "mail here",
                Password = "password here",
                Settings = Settings.All
            });

            /*=============================Тянем фото со стены================================*/            
            var getPosts = api.Wall.Get(new WallGetParams
            {
                OwnerId = groupId,
                Filter = WallFilter.Owner,
                Extended = true,
                Count  = 1 
            });

            offsertPostExit = (int)(getPosts.TotalCount + (100 - getPosts.TotalCount % 100));
            Console.WriteLine(getPosts.TotalCount);
            
            for (int i = 0; i < offsertPostExit; i += 100)
            {
                getPosts = api.Wall.Get(new WallGetParams
                {
                    OwnerId = groupId,
                    Filter = WallFilter.Owner,
                    Extended = true,
                    Count = 100,
                    Offset = (ulong)i
                });
                
                foreach (var gPos in getPosts.WallPosts)
                {
                    var photoDetector = 0;

                    foreach (var x in gPos.Attachments)
                    {
                        if (x.Type.ToString().Equals("VkNet.Model.Attachments.Photo"))
                        {
                            photoDetector++;
                        }
                    }

                    if (photoDetector > 0)
                    {
                        var fsModel = new wallPhotoModel
                        {
                            PostLink = linkHead + "wall" + groupId + "_" + gPos.Id, //ссылка на пост +
                            PostCreateTime = gPos.Date.Value.AddHours(2).ToString(),//дата создания записи +
                            PhotoLink = fromServices.GetPhotoLink(gPos), //ссылка на фото в вк +
                            PostLikes = gPos.Likes.Count.ToString(),//лайки к фото+
                            PostShares = gPos.Reposts.Count.ToString(),//репосты к фото+
                            PostComents = fromServices.GetCommentsOfPost(gPos.Id, api, groupId) //коменты и их авторы +
                        };

                        wallPhotoList.Add(fsModel);
                        Console.WriteLine(countPosts + " // " + gPos.Id);
                        countPosts++;
                        //пишем в файл модели постов
                        using (StreamWriter streamReaderWall = new StreamWriter("wallPhotoList.csv"))
                        {
                            using (CsvWriter csvReader = new CsvWriter(streamReaderWall))
                            {
                                csvReader.Configuration.Delimiter = ";";
                                csvReader.WriteRecords(wallPhotoList);
                            }
                        }
                    }
                            
                }                                       
            }

            /*=============================Тянем фото с альбома================================*/             
            var albums = api.Photo.GetAlbums(new PhotoGetAlbumsParams
                {
                    OwnerId = groupId,
                });

                Console.WriteLine(albums.Count.ToString());
                //int countAlbumPhotos = 0;
                //проходимся по всем альбомам
                foreach (var a in albums)
                {
                    //кидаем запрос чтобы узнать сколько всего фото в альбоме
                    var albumPhotos = api.Photo.Get(new PhotoGetParams
                    {
                        OwnerId = groupId,
                        AlbumId = PhotoAlbumType.Id(a.Id)
                    });

                    //округляем ко-во фото в альбоме
                    offsetPhotoExit = (int)(albumPhotos.TotalCount + (100 - albumPhotos.TotalCount % 100));
                    //countAlbumPhotos = countAlbumPhotos + (int)albumPhotos.TotalCount;
                    Console.WriteLine(albumPhotos.TotalCount);
                //делаем цикл с шагом 100 чтобы пройти по всем фото
                
                for (var j = 0; j < offsetPhotoExit; j += 100)
                    {
                        //кидаем уже подробный запрос по фоткам
                        albumPhotos = api.Photo.Get(new PhotoGetParams
                        {
                            OwnerId = groupId,
                            AlbumId = PhotoAlbumType.Id(a.Id),
                            Reversed = true,
                            Extended = true,
                            PhotoSizes = true,
                            Count = 100,
                            Offset = (ulong)j,
                        });

                        //проходимся по фоткам в альбома
                        foreach (var ap in albumPhotos)
                        {
                            //заполняем модель
                            var apModel = new albumPhotoModel
                            {
                                AlbumId = a.Id.ToString(),
                                AlbumName = a.Title,
                                PhotoDate = ap.CreateTime.Value.AddHours(2).ToString(),
                                PhotoLink = linkHead + "photo" + groupId + ap.Id,
                                PhotoUri = fromServices.GetUrlOfBigPhoto(ap),
                                PhotoLikes = ap.Likes.Count.ToString(),
                                //PhotoShares = "reposts here",
                                //PhotoComents = fromServices.GetCommentsOfPost(ap.PostId, api, groupId)
                            };

                            //добавляем модель в список
                            albumPhotoList.Add(apModel);
                            Console.WriteLine(countPhotos + " // " + ap.Id);
                            countPhotos++;

                            //пишем в файл модели фоток
                            using (StreamWriter streamReaderAlbum = new StreamWriter("albumPhotoList.csv"))
                            {
                                using (CsvWriter csvReader = new CsvWriter(streamReaderAlbum))
                                {
                                    csvReader.Configuration.Delimiter = ";";
                                    csvReader.WriteRecords(albumPhotoList);
                                }
                            }
                        }
                    }
                }
                
            //Console.WriteLine("Count photos in all albums " + countAlbumPhotos);

            /*=============================Сохраняем фото со стены================================*/           
            var getPhotos = api.Photo.Get(new PhotoGetParams
            {
                OwnerId = groupId,
                AlbumId = VkNet.Enums.SafetyEnums.PhotoAlbumType.Wall,
            });

            offsetWallPhotoSave = (int)(getPhotos.TotalCount + (1000 - getPhotos.TotalCount % 1000));
            Console.WriteLine(getPhotos.TotalCount);

            for (var i = 0; i < offsetWallPhotoSave; i += 1000)
            {
                getPhotos = api.Photo.Get(new PhotoGetParams
                {
                    OwnerId = groupId,
                    AlbumId = VkNet.Enums.SafetyEnums.PhotoAlbumType.Wall,
                    Reversed = true,
                    Extended = true,
                    PhotoSizes = true,
                    Count = 1000,
                    Offset = (ulong)i
                });

                using (webClient)
                {
                    foreach (var gp in getPhotos)
                    {
                        webClient.DownloadFile(fromServices.GetUrlOfBigPhoto(gp), "wallPhotos\\" + gp+".jpg"); 
                        Console.WriteLine(countPhotosWSave +" // " + linkHead + gp);
                        countPhotosWSave++;
                    }
                }
 
            }           

            /*=============================Сохраняем фото с альбомов================================*/
            var albumsForSave = api.Photo.GetAlbums(new PhotoGetAlbumsParams
            {
                OwnerId = groupId,
            });

            Console.WriteLine(albumsForSave.Count.ToString());

            foreach (var a in albumsForSave)
            {
                Directory.CreateDirectory("albumPhotos\\" + a.Id);
                Console.WriteLine(" Создан albumPhotos\\" + a.Id);
            }

            foreach (var a in albumsForSave)
            {
                var getPhotosSave = api.Photo.Get(new PhotoGetParams
                {
                    OwnerId = groupId,
                    AlbumId = PhotoAlbumType.Id(a.Id)
                });

                offsetAlbumPhotoSave = (int)(getPhotosSave.TotalCount + (1000 - getPhotosSave.TotalCount % 1000));
                countPhotosASave = countPhotosASave + (int)getPhotosSave.TotalCount;
                Console.WriteLine(getPhotosSave.TotalCount);

                for (var i = 0; i < offsetAlbumPhotoSave; i += 1000)
                {
                    getPhotosSave = api.Photo.Get(new PhotoGetParams
                    {
                        OwnerId = groupId,
                        AlbumId = PhotoAlbumType.Id(a.Id),
                        Reversed = true,
                        Extended = true,
                        PhotoSizes = true,
                        Count = 1,
                        Offset = (ulong)i
                    });

                    using (webClient)
                    {
                        foreach (var gp in getPhotosSave)
                        {
                            webClient.DownloadFile(fromServices.GetUrlOfBigPhoto(gp), "albumPhotos\\" + a.Id + "\\" + gp + ".jpg");
                            Console.WriteLine(countPhotosASave + " // " + linkHead + gp);
                            countPhotosASave++;
                        }
                    }
                }
            }

            Console.WriteLine("Всего фоток " + countPhotosASave);
            Console.WriteLine( "END PROGRAM//PRESS ANY KEY");
            Console.ReadKey();
        }

    }
}
