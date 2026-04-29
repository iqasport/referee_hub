import { urlType } from "./socialUtils";

describe("urlType", () => {
  describe("Facebook", () => {
    it("returns facebook for facebook.com", () => {
      expect(urlType("https://www.facebook.com/mypage")).toBe("facebook");
    });

    it("returns facebook for fb.com", () => {
      expect(urlType("https://fb.com/mypage")).toBe("facebook");
    });

    it("returns facebook for fb.me", () => {
      expect(urlType("https://fb.me/mypage")).toBe("facebook");
    });
  });

  describe("Twitter / X", () => {
    it("returns twitter for twitter.com", () => {
      expect(urlType("https://twitter.com/handle")).toBe("twitter");
    });

    it("returns twitter for x.com", () => {
      expect(urlType("https://x.com/handle")).toBe("twitter");
    });
  });

  describe("Instagram", () => {
    it("returns instagram for instagram.com", () => {
      expect(urlType("https://www.instagram.com/profile")).toBe("instagram");
    });

    it("returns instagram for ig.me", () => {
      expect(urlType("https://ig.me/profile")).toBe("instagram");
    });
  });

  describe("YouTube", () => {
    it("returns youtube for youtube.com", () => {
      expect(urlType("https://www.youtube.com/channel/abc")).toBe("youtube");
    });

    it("returns youtube for youtu.be", () => {
      expect(urlType("https://youtu.be/abc123")).toBe("youtube");
    });
  });

  describe("Other", () => {
    it("returns other for unrecognized URLs", () => {
      expect(urlType("https://www.linkedin.com/in/someone")).toBe("other");
    });

    it("returns other for empty string", () => {
      expect(urlType("")).toBe("other");
    });
  });
});
