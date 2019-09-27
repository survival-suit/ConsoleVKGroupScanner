namespace ConsoleVKGroupScanner
{
    public class albumPhotoModel
    {
        public string AlbumId { get; set; } //ИД альбома
        public string AlbumName { get; set; } //имя альбома
        public string PhotoDate { get; set; } //дата публикации фото
        public string PhotoLink { get; set; } //линк на фотoграфию
        public string PhotoUri { get; set; } //uri на фотoграфию
        public string PhotoLikes { get; set; } //число лайков
        //public string PhotoShares { get; set; } //число репостов
        //public string PhotoComents { get; set; } //комменты и их авторы  
    }
}
