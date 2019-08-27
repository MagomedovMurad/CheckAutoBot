using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CheckAutoBot.Vk.Api.Enums
{
    public enum EventType
    {
        /// <summary>
        /// Входящее сообщение
        /// </summary>
        [EnumMember(Value = "message_new")]
        NewMessage,

        /// <summary>
        /// Новое исходящее сообщение
        /// </summary>
        [EnumMember(Value = "message_reply")]
        ReplyMessage,

        /// <summary>
        /// Редактирование сообщения
        /// </summary>
        [EnumMember(Value = "message_edit")]
        EditMessage,

        /// <summary>
        /// Подписка на сообщения от сообщества
        /// </summary>
        [EnumMember(Value = "message_allow")]
        AllowMessage,

        /// <summary>
        /// Новый запрет сообщений от сообщества
        /// </summary>
        [EnumMember(Value = "message_deny")]
        DenyMessage,

        /// <summary>
        /// Добавление фотографии
        /// </summary>
        [EnumMember(Value = "photo_new")]
        NewFoto,

        /// <summary>
        /// Добавление комментария к фотографии
        /// </summary>
        [EnumMember(Value = "photo_comment_new")]
        NewPhotoComment,

        /// <summary>
        /// Редактирование комментария к фотографии
        /// </summary>
        [EnumMember(Value = "photo_comment_edit")]
        EditPhotoComment,

        /// <summary>
        /// Восстановление комментария к фотографии
        /// </summary>
        [EnumMember(Value = "photo_comment_restore")]
        RestorePhotoComment,

        /// <summary>
        /// Удаление комментария к фотографии
        /// </summary>
        [EnumMember(Value = "photo_comment_delete")]
        DeletePhotoComment,

        /// <summary>
        /// Добавление аудио
        /// </summary>
        [EnumMember(Value = "audio_new")]
        NewAudio,

        /// <summary>
        /// Добавление видео
        /// </summary>
        [EnumMember(Value = "video_new")]
        NewVideo,

        /// <summary>
        /// Комментарий к видео
        /// </summary>
        [EnumMember(Value = "video_comment_new")]
        NewVideoComment,

        /// <summary>
        /// Редактирование комментария к видео
        /// </summary>
        [EnumMember(Value = "video_comment_edit")]
        EditVideoComment,

        /// <summary>
        /// Восстановление комментария к видео
        /// </summary>
        [EnumMember(Value = "video_comment_restore")]
        RestoreVideoComment,

        /// <summary>
        /// Удаление комментария к видео
        /// </summary>
        [EnumMember(Value = "video_comment_delete")]
        DeleteVideoComment,

        /// <summary>
        /// Запись на стене
        /// </summary>
        [EnumMember(Value = "wall_post_new")]
        NewWallPost,

        /// <summary>
        /// Репост записи из сообщества
        /// </summary>
        [EnumMember(Value = "wall_repost")]
        WallRepost,

        /// <summary>
        /// Добавление комментария на стене
        /// </summary>
        [EnumMember(Value = "wall_reply_new")]
        NewWallReply,

        /// <summary>
        /// Редактирование комментария на стене 
        /// </summary>
        [EnumMember(Value = "wall_reply_edit")]
        EditWallReply,

        /// <summary>
        /// Восстановление комментария на стене
        /// </summary>
        [EnumMember(Value = "wall_reply_restore")]
        RestoreWallReply,

        /// <summary>
        /// Удаление комментария на стене
        /// </summary>
        [EnumMember(Value = "wall_reply_delete")]
        DeleteWallReply,

        /// <summary>
        /// Создание комментария в обсуждении
        /// </summary>
        [EnumMember(Value = "board_post_new")]
        NewBoardPost,

        /// <summary>
        /// Редактирование комментария
        /// </summary>
        [EnumMember(Value = "board_post_edit")]
        EditBoardPost,

        /// <summary>
        /// Восстановление комментария
        /// </summary>
        [EnumMember(Value = "board_post_restore")]
        RestoreBoardPost,

        /// <summary>
        /// Удаление комментария в обсуждении
        /// </summary>
        [EnumMember(Value = "board_post_delete")]
        DeleteBoardPost,

        /// <summary>
        /// Новый комментарий к товару
        /// </summary>
        [EnumMember(Value = "market_comment_new ")]
        NewMarketComment,

        /// <summary>
        /// Редактирование комментария к товару
        /// </summary>
        [EnumMember(Value = "market_comment_edit")]
        EditMarketComment,

        /// <summary>
        /// Восстановление комментария к товару
        /// </summary>
        [EnumMember(Value = "market_comment_restore")]
        RestoreMarketComment,

        /// <summary>
        /// Удаление комментария к товару
        /// </summary>
        [EnumMember(Value = "market_comment_delete")]
        DeleteMarketComment,

        /// <summary>
        /// Удаление участника из сообщества
        /// </summary>
        [EnumMember(Value = "group_leave")]
        GroupLeave,

        /// <summary>
        /// Добавление участника или заявки на вступление в сообщество
        /// </summary>
        [EnumMember(Value = "group_join")]
        GroupJoin,

        /// <summary>
        /// Добавление пользователя в чёрный список
        /// </summary>
        [EnumMember(Value = "user_block")]
        BlockUser,

        /// <summary>
        /// Удаление пользователя из чёрного списка
        /// </summary>
        [EnumMember(Value = "user_unblock")]
        UnblockUser,

        /// <summary>
        /// Добавление голоса в публичном опросе
        /// </summary>
        [EnumMember(Value = "poll_vote_new")]
        NewPollVote,

        /// <summary>
        /// Редактирование списка руководителей
        /// </summary>
        [EnumMember(Value = "group_officers_edit")]
        EditGroupOfficers,

        /// <summary>
        /// Изменение настроек сообщества
        /// </summary>
        [EnumMember(Value = "group_change_settings")]
        ChangeGroupSettings,

        /// <summary>
        /// Изменение главного фото
        /// </summary>
        [EnumMember(Value = "group_change_photo")]
        ChangeGroupPhoto
    }
}
