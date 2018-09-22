class NationalGoverningBodySerializer
  include FastJsonapi::ObjectSerializer

  attributes :name,
             :website
end
