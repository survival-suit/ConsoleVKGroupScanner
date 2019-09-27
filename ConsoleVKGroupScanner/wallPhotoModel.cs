namespace ConsoleVKGroupScanner
{
        public class wallPhotoModel
    {
            public string PostLink { get; set; } //линк на пост
            public string PostCreateTime { get; set; } //дата публикации
            public string PhotoLink { get; set; } //линк на фотoграфию
            //public string PhotoLinkURI { get; set; } //uri на фотoграфию
            public string PostLikes { get; set; } //число лайков
            public string PostShares { get; set; } //число репостов
            public string PostComents { get; set; } //комменты и их авторы  
    }
}
