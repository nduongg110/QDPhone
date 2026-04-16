import { Badge, Col } from 'antd'
import React from 'react'
import { WrapperHeader, WrapperTextHeader, WarpHeaderAccount, WrapperTextHeaderSmall } from './style'
import { UserOutlined, CaretDownOutlined, ShoppingCartOutlined } from '@ant-design/icons'
import ButttonInputSearch from '../ButtoninputSearch/ButtoninputSearch'

const HeaderComponent = () => {
  return (
    <div style={{width: '100%', background: '#006400', display: 'flex', justifyContent: 'center'}}>
      <WrapperHeader>
        <Col span={5}>
          <WrapperTextHeader>QDPHONE</WrapperTextHeader>
        </Col>
        <Col span={13}>
        <ButttonInputSearch
          size="large"
          textButton="Tìm kiếm"
          placeholder="Nhập sản phẩm bạn muốn tìm"
          // onSearch={onSearch}

        />

        </Col>
        <Col span={6} style = {{display: 'flex', gap: '54px', alignItems: 'center'}}>
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
            <Badge count={4} size="small">
              <ShoppingCartOutlined style={{ fontSize: '30px', color: '#fff' }}/>
            </Badge>
              <WrapperTextHeaderSmall>Giỏ hàng</WrapperTextHeaderSmall>
          </div>
        </Col>
        </WrapperHeader>
    </div>
  )
}

export default HeaderComponent
