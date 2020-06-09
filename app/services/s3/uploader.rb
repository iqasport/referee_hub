module Services
  module S3
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

        "https://#{bucket_name}.s3.amazonaws.com/#{key}"
      end

      private

      def storage
        Fog.mock! if Rails.env.test? || Rails.env.development?
        
        @storage ||= Fog::Storage.new(
          provider: 'AWS',
          aws_access_key_id: aws_access_key_id,
          aws_secret_access_key: aws_secret_access_key,
          region: 'eu-central-1'
        )
      end

      def bucket
        @bucket ||= begin
          if storage.directories.blank?
            # ensure the bucket is available to upload the file into
            storage.put_bucket(bucket_name)
          end

          storage.directories.get(bucket_name)
        end
      end

      def aws_access_key_id
        return 'nonsense' if Rails.env.test? || Rails.env.development?

        ENV['AWS_ACCESS_KEY_ID']
      end

      def aws_secret_access_key
        return 'nonsense' if Rails.env.test? || Rails.env.development?

        ENV['AWS_SECRET_ACCESS_KEY']
      end

      def bucket_name
        return 'nonsense' if Rails.env.test? || Rails.env.development?

        ENV['AWS_BUCKET']
      end
    end
  end
end
