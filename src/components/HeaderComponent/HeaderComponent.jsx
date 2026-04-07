import { Col } from 'antd'
import React from 'react'
import { WrapperHeader } from './style'

const HeaderComponent = () => {
  return (
    <div>
      <WrapperHeader>
        <Col span={6}>col-6</Col>
        <Col span={12}>col-12</Col>
        <Col span={6}>col-6</Col>
      </WrapperHeader>
    </div>
  )
}

export default HeaderComponent
