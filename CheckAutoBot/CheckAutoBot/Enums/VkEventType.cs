using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot.Enums
{
    public enum VkEventType
    {
        /// <summary>
        /// Входящее сообщение
        /// </summary>
        [JsonProperty("message_new")]
        NewMessage,

        /// <summary>
        /// Новое исходящее сообщение
        /// </summary>
        [JsonProperty("message_reply")]
        ReplyMessage,

        /// <summary>
        /// Редактирование сообщения
        /// </summary>
        [JsonProperty("message_edit")]
        EditMessage,

        /// <summary>
        /// Подписка на сообщения от сообщества
        /// </summary>
        [JsonProperty("message_allow")]
        AllowMessage,

        /// <summary>
        /// Новый запрет сообщений от сообщества
        /// </summary>
        [JsonProperty("message_deny")]
        DenyMessage,

        /// <summary>
        /// Добавление фотографии
        /// </summary>
        [JsonProperty("photo_new")]
        NewFoto,

        /// <summary>
        /// Добавление комментария к фотографии
        /// </summary>
        [JsonProperty("photo_comment_new")]
        NewPhotoComment,

        /// <summary>
        /// Редактирование комментария к фотографии
        /// </summary>
        [JsonProperty("photo_comment_edit")]
        EditPhotoComment,

        /// <summary>
        /// Восстановление комментария к фотографии
        /// </summary>
        [JsonProperty("photo_comment_restore")]
        RestorePhotoComment,

        /// <summary>
        /// Удаление комментария к фотографии
        /// </summary>
        [JsonProperty("photo_comment_delete")]
        DeletePhotoComment,

        /// <summary>
        /// Добавление аудио
        /// </summary>
        [JsonProperty("audio_new")]
        NewAudio,

        /// <summary>
        /// Добавление видео
        /// </summary>
        [JsonProperty("video_new")]
        NewVideo,

        /// <summary>
        /// Комментарий к видео
        /// </summary>
        [JsonProperty("video_comment_new")]
        NewVideoComment,

        /// <summary>
        /// Редактирование комментария к видео
        /// </summary>
        [JsonProperty("video_comment_edit")]
        EditVideoComment,

        /// <summary>
        /// Восстановление комментария к видео
        /// </summary>
        [JsonProperty("video_comment_restore")]
        RestoreVideoComment,

        /// <summary>
        /// Удаление комментария к видео
        /// </summary>
        [JsonProperty("video_comment_delete")]
        DeleteVideoComment,

        /// <summary>
        /// Запись на стене
        /// </summary>
        [JsonProperty("wall_post_new")]
        NewWallPost,

        /// <summary>
        /// Репост записи из сообщества
        /// </summary>
        [JsonProperty("wall_repost")]
        WallRepost,

        /// <summary>
        /// Добавление комментария на стене
        /// </summary>
        [JsonProperty("wall_reply_new")]
        NewWallReply,

        /// <summary>
        /// Редактирование комментария на стене 
        /// </summary>
        [JsonProperty("wall_reply_edit")]
        EditWallReply,

        /// <summary>
        /// Восстановление комментария на стене
        /// </summary>
        [JsonProperty("wall_reply_restore")]
        RestoreWallReply,

        /// <summary>
        /// Удаление комментария на стене
        /// </summary>
        [JsonProperty("wall_reply_delete")]
        DeleteWallReply,

        /// <summary>
        /// Создание комментария в обсуждении
        /// </summary>
        [JsonProperty("board_post_new")]
        NewBoardPost,

        /// <summary>
        /// Редактирование комментария
        /// </summary>
        [JsonProperty("board_post_edit")]
        EditBoardPost,

        /// <summary>
        /// Восстановление комментария
        /// </summary>
        [JsonProperty("board_post_restore")]
        RestoreBoardPost,

        /// <summary>
        /// Удаление комментария в обсуждении
        /// </summary>
        [JsonProperty("board_post_delete")]
        DeleteBoardPost,

        /// <summary>
        /// Новый комментарий к товару
        /// </summary>
        [JsonProperty("market_comment_new ")]
        NewMarketComment,

        /// <summary>
        /// Редактирование комментария к товару
        /// </summary>
        [JsonProperty("market_comment_edit")]
        EditMarketComment,

        /// <summary>
        /// Восстановление комментария к товару
        /// </summary>
        [JsonProperty("market_comment_restore")]
        RestoreMarketComment,

        /// <summary>
        /// Удаление комментария к товару
        /// </summary>
        [JsonProperty("market_comment_delete")]
        DeleteMarketComment,

        /// <summary>
        /// Удаление участника из сообщества
        /// </summary>
        [JsonProperty("group_leave")]
        GroupLeave,

        /// <summary>
        /// Добавление участника или заявки на вступление в сообщество
        /// </summary>
        [JsonProperty("group_join")]
        GroupJoin,

        /// <summary>
        /// Добавление пользователя в чёрный список
        /// </summary>
        [JsonProperty("user_block")]
        BlockUser,

        /// <summary>
        /// Удаление пользователя из чёрного списка
        /// </summary>
        [JsonProperty("user_unblock")]
        UnblockUser,

        /// <summary>
        /// Добавление голоса в публичном опросе
        /// </summary>
        [JsonProperty("poll_vote_new")]
        NewPollVote,

        /// <summary>
        /// Редактирование списка руководителей
        /// </summary>
        [JsonProperty("group_officers_edit")]
        EditGroupOfficers,

        /// <summary>
        /// Изменение настроек сообщества
        /// </summary>
        [JsonProperty("group_change_settings")]
        ChangeGroupSettings,

        /// <summary>
        /// Изменение главного фото
        /// </summary>
        [JsonProperty("group_change_photo")]
        ChangeGroupPhoto

    }
}
