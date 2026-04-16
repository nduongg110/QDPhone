import React from 'react'
import { StyleNameProduct, WrapperDiscountText, WrapperPriceText, WrapperReportText, WrapperStyleTextSell } from './style'
import { StarFilled } from '@ant-design/icons'
import { WrapperCardStyle } from '../CardComponent/style'
import logo from '../../assets/images/logo.png'


const CardComponent = () => {
  return (
        <WrapperCardStyle
        hoverable
        headStyle={{ width: '200px', height: '200px' }}
        style={{ width: 200 }}
        bodyStyle={{ padding: '10px' }}
        cover={<img alt="example" src="https://os.alipayobjects.com/rmsportal/QBnOOoLaAfKPirc.png" />}
        >
        <img
        src={logo}
        style={{
            width: '68px', 
            height: '14px', 
            position: 'absolute', 
            top: -1, 
            left: -1,
            borderTopLeftRadius: '3px'
        }}
        />


      <StyleNameProduct>Iphone</StyleNameProduct>
      <WrapperReportText>
        <span style={{marginRight: '4px'}}>
          <span>4.96 </span><StarFilled style={{ fontSize: '12px', color: 'yellow' }} />
        </span>
        <WrapperStyleTextSell> | Đã bán: 1000+</WrapperStyleTextSell>
      </WrapperReportText>
      <WrapperPriceText>
        <span style={{ marginRight: '8px' }}>1.000.000đ</span>
        <WrapperDiscountText style={{paddingLeft: '25px'}}>
          -5%
        </WrapperDiscountText>
      </WrapperPriceText>
    </WrapperCardStyle>
  )
}
export default CardComponent   