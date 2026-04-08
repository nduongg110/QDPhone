import { Button, Input } from 'antd'
import React from 'react'
import { SearchOutlined } from '@ant-design/icons'

const ButttonInputSearch = (props) => {
    const { 
      size, placeholder, textButton, bordered, 
      backgroundColorInput = '#fff', 
      backgroundColorButton = '#008000',
      colorButton = '#fff'
        } = props
    return (
    <div style={{ display: 'flex' }}>
    <Input
      size={size}
      placeholder={placeholder}
      bordered={bordered}
      style={{ backgroundColor: backgroundColorInput, borderRadius: 0 }}
    />
    <Button
      size={size}
      style={{ background: backgroundColorButton, border: !bordered && 'none', borderRadius: 0 }}
      icon={<SearchOutlined style={{ color: colorButton }}/>}
    >
      <span style={{ color: colorButton }}>{textButton}</span>
    </Button>
  </div>



  )
}

export default ButttonInputSearch
