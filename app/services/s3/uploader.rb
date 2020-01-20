module Services
  class Uploader
    InvalidData = Class.new(StandardError)
    InvalidExtension = Class.new(StandardError)
    InvalidKey = Class.new(StandardError)

    DEFAULT_ENCRYPTION = 'AES256'.freeze

    attr_accessor :data, :content_type, :extension, :public_access, :encryption, :key

    def initialize(data:, content_type:, extension:, public_access:, key:)
      @data = data
      @content_type = content_type
      @extension = extension
      @public = public_access
      @encryption = DEFAULT_ENCRYPTION
      @key = key
    end

    def perform
      raise InvalidData, 'Missing data to upload' if data.blank?
      raise InvalidExtension, 'Missing extension type for upload' if extension.blank?
      raise InvalidKey, 'Missing directory or key' if key.blank?

      bucket.files.create(
        key: key,
        body: data,
        content_type: content_type,
        encryption: encryption,
        public: public_access
      )

      "https://#{ENV['AWS_BUCKET']}.s3.amazonaws.com/#{key}"
    end

    private

    def storage
      @storage ||= Fog::Storage.new(
        provider: 'AWS',
        aws_access_key_id: ENV['AWS_ACCESS_KEY_ID'],
        aws_secret_access_key: ENV['AWS_SECRET_ACCESS_KEY'],
        region: 'eu-central-1'
      )
    end

    def bucket
      @bucket ||= storage.directories.get(ENV['AWS_BUCKET'])
    end

    # def create_key
    #   current_time = Time.now
    #   iso_time = ("%10.5f" % current_time.to_f).to_i
    #   key.present? ? key : "#{directory}/#{iso_time}.#{extension}"
    # end
  end
end
