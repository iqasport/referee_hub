namespace ManagementHub.Models.Abstraction.Contexts.Options;

public class GetUserContextOptions
{
    /// <summary>
    /// Indicates wether the user context should load the password hash from the database into application memory.
    /// For security reasons it's best to only load it during sign in.
    /// </summary>
    public bool LoadPasswordHash { get; set; } = false;

    /// <summary>
    /// Indicates wether the user context should load the password reset token from the database into application memory.
    /// For security reasons it's best to only load it during password reset verification.
    /// </summary>
    public bool LoadPasswordResetToken { get; set; } = false;

    /// <summary>
    /// Indicates wether the user context should load the email confirmation token from the database into application memory.
    /// For security reasons it's best to only load it during email verification callback.
    /// </summary>
    public bool LoadEmailConfirmationToken { get; set; } = false;

    /// <summary>
    /// Indicates wether the user context should load the invitation token from the database into application memory.
    /// For security reasons it's best to only load it during email invitation callback.
    /// </summary>
    public bool LoadInvitationToken { get; set; } = false;

    /// <summary>
    /// Indicates wether to preload information on password reset. Otherwise it will be available lazily.
    /// </summary>
    public bool PreloadPasswordReset { get; set; } = false;

    /// <summary>
    /// Indicates wether to preload information on user confirmation. Otherwise it will be available lazily.
    /// </summary>
    public bool PreloadUserConfirmation { get; set; } = true;

    /// <summary>
    /// Indicates wether to preload information on invitation. Otherwise it will be available lazily.
    /// </summary>
    public bool PreloadInvitation { get; set; } = false;
}
