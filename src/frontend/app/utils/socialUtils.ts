import { SocialAccountType } from "../store/serviceApi";

export const urlType = (url: string): SocialAccountType => {
    if (url.includes("facebook.com") || url.includes("fb.com") || url.includes("fb.me")) {
        return "facebook";
    }
    else if (url.includes("twitter.com") || url.includes("x.com")) {
        return "twitter";
    }
    else if (url.includes("instagram.com") || url.includes("ig.me")) {
        return "instagram";
    }
    else if (url.includes("youtube.com") || url.includes("youtu.be")) {
        return "youtube";
    }

    return "other";
}