using Rise.Shared.Enums;

namespace Rise.Shared.Notifications
{
    /// <summary>
    /// Defines methods for managing notifications.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Retrieves all notifications.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of notifications.</returns>
        Task<IEnumerable<NotificationDto.ViewNotification>?> GetAllNotificationsAsync(String language = "en");
        /// <summary>
        /// Retrieves a notification by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the notification.</param>
        /// <param name="language">The language of the notification.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the notification.</returns>
        Task<NotificationDto.ViewNotification?> GetNotificationById(String id, String language = "en");
        /// <summary>
        /// Creates a new notification.
        /// </summary>
        /// <param name="notification">The notification to create.</param>
        /// <param name="language">The language of the notification.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created notification.</returns>
        Task<NotificationDto.ViewNotification> CreateNotificationAsync(NotificationDto.NewNotification notification, String language = "en");
        /// <summary>
        /// Updates an existing notification.
        /// </summary>
        /// <param name="notification">The notification to update.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the update was successful.</returns>
        Task<Boolean> UpdateNotificationAsync(NotificationDto.UpdateNotification notification);
        /// <summary>
        /// Deletes a notification by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the notification to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the deletion was successful.</returns>
        Task<Boolean> DeleteNotificationAsync(String id);

        /// <summary>
        /// Retrieves all notifications for a specific user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="language">The language of the notification.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of notifications.</returns>
        Task<IEnumerable<NotificationDto.ViewNotification>?> GetAllUserNotifications(String userId, String language = "en");

        /// <summary>
        /// Retrieves all unread notifications for a specific user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="language">The language of the notification.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of unread notifications.</returns>
        Task<IEnumerable<NotificationDto.ViewNotification>?> GetUnreadUserNotifications(String userId, String language = "en");
        /// <summary>
        /// Retrieves all read notifications for a specific user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="language">The language of the notification.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of read notifications.</returns>
        Task<IEnumerable<NotificationDto.ViewNotification>?> GetReadUserNotifications(String userId, String language = "en");
        /// <summary>
        /// Retrieves notifications of a specific type for a specific user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="type">The type of notifications to retrieve.</param>
        /// <param name="language">The language of the notification.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of notifications of the specified type.</returns>
        Task<IEnumerable<NotificationDto.ViewNotification>?> GetUserNotificationsByType(String userId, NotificationType type, String language = "en");
    }
}