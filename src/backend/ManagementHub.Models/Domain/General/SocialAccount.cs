using System;
using ManagementHub.Models.Enums;

namespace ManagementHub.Models.Domain.General;

public record SocialAccount(Uri Url, SocialAccountType Type);
