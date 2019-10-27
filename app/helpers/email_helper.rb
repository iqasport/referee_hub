module EmailHelper
  def email_image_tag(image, **options)
    filename = Rails.root.to_s + "/app/assets/images/#{image}"

    attachments[image] = {
      data: File.read(filename),
      mime_type: "image/#{image.split('.').last}"
    }

    image_tag attachments[image].url, **options
  end
end
