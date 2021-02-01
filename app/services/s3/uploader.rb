module Services
  module S3
    class Uploader
      InvalidData = Class.new(StandardError)
      InvalidExtension = Class.new(StandardError)
      InvalidKey = Class.new(StandardError)

      DEFAULT_ENCRYPTION = 'AES256'.freeze

      attr_accessor :data, :content_type, :extension, :key, :bucket_name

      def initialize(data:, content_type:, extension:, key:, bucket_name:)
        @data = data
        @content_type = content_type
        @extension = extension
        @key = key
        @bucket_name = bucket_name
      end

      def perform
        raise InvalidData, 'Missing data to upload' if data.blank?
        raise InvalidExtension, 'Missing extension type for upload' if extension.blank?
        raise InvalidKey, 'Missing directory or key' if key.blank?

        bucket.files.create(
          key: key,
          body: data,
          content_type: content_type,
          encryption: DEFAULT_ENCRYPTION,
          public: true
        )

        "https://#{bucket_name}.s3.amazonaws.com/#{key}"
      end

      private

      def storage
        Fog.mock! if Rails.env.test? || Rails.env.development?

        @storage ||= Fog::Storage.new(
          provider: 'AWS',
          aws_access_key_id: aws_access_key_id,
          aws_secret_access_key: aws_secret_access_key
        )
      end

      def bucket
        fetched_bucket = storage.directories.get(bucket_name)
        return fetched_bucket unless fetched_bucket.blank?

        storage.directories.create(key: bucket_name, public: true)
      end

      def aws_access_key_id
        return 'nonsense' if Rails.env.test? || Rails.env.development?

        ENV['AWS_ACCESS_KEY_ID']
      end

      def aws_secret_access_key
        return 'nonsense' if Rails.env.test? || Rails.env.development?

        ENV['AWS_SECRET_ACCESS_KEY']
      end
    end
  end
end
