require 'rails_helper'

RSpec.describe Link, type: :model do
  let(:link) { build :link }

  subject { link.save }

  it 'is a valid object' do
    expect(link).to be_valid
    expect { subject }.to_not raise_error
  end
end
