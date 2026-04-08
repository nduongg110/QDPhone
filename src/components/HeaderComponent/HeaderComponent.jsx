import { Col } from 'antd'
import React from 'react'
import { WrapperHeader, WrapperTextHeader, WarpHeaderAccount, WrapperTextHeaderSmall } from './style'
import { UserOutlined, CaretDownOutlined, ShoppingCartOutlined } from '@ant-design/icons'
import ButttonInputSearch from '../ButtoninputSearch/ButtoninputSearch'

const HeaderComponent = () => {
  return (
    <div>
      <WrapperHeader gutter={16}>
        <Col span={6}>
          <WrapperTextHeader>QDPHONE</WrapperTextHeader>
        </Col>
        <Col span={12}>
        <ButttonInputSearch
          size="large"
          textButton="Tìm kiếm"
          placeholder="Nhập sản phẩm bạn muốn tìm"
          // onSearch={onSearch}

        />

        </Col>
        <Col span={6} style = {{display: 'flex', gap: '20px', alignItems: 'center'}}>
          <WarpHeaderAccount>
            <UserOutlined style={{ fontSize: '30px' }} />
          <div>
            <WrapperTextHeaderSmall style={{ fontSize: '12px' }}>Đăng nhập/Đăng ký</WrapperTextHeaderSmall>
          <div>
              <WrapperTextHeaderSmall style={{ fontSize: '12px' }}>Tài khoản</WrapperTextHeaderSmall>
              <CaretDownOutlined />
          </div>
          </div>
          </WarpHeaderAccount>
          <div>
              <ShoppingCartOutlined style={{ fontSize: '30px', color: '#fff' }} />
              <WrapperTextHeaderSmall>Giỏ hàng</WrapperTextHeaderSmall>
          </div>
        </Col>
        </WrapperHeader>
    </div>
  )
}

export default HeaderComponent
