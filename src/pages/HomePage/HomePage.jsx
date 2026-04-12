import React from 'react'
import TypeProduct from '../../components/TypeProduct/TypeProduct'
import { WrapperButtonMore, WrapperProducts, WrapperTypeProduct } from './style'
import banner1 from '../../assets/images/banner1.webp'
import banner2 from '../../assets/images/banner2.jpg'
import banner3 from '../../assets/images/banner3.jpg'
import banner4 from '../../assets/images/banner4.jpg'
import SlideComponent from '../../components/SliderComponent/SliderComponent'
import CardComponent from '../../components/CardComponent/CardComponent'

const HomePage = () => {
  const arr = ['Điện thoại thông minh', 'Máy tính bảng', 'Phụ kiện điện thoại']
  return (
    <>
    <div style={{ width: '1270px',margin: '0 auto' }}>
      <WrapperTypeProduct>
        {arr.map((item) => {
          return (
            <TypeProduct name={item} key={item} />
          )
        })}
      </WrapperTypeProduct>
    </div>
  <div className='body' style={{ width: '100%', backgroundColor: '#efefef' }}>
    <div id='container' style={{ height: '1000px', width: '1270px', margin: '0 auto' }}>
      <SlideComponent arrImages={[banner1, banner2, banner3, banner4]} />
      <WrapperProducts>
        <CardComponent />
        <CardComponent />
        <CardComponent />
        <CardComponent />
        <CardComponent />
        <CardComponent />
      </WrapperProducts>
      <div style={{ width: '100%', display: 'flex', justifyContent: 'center', marginTop: '10px' }}>
        <WrapperButtonMore textButton='Xem thêm' type='outline' styleButton={{
          border: '1px solid rgb(11, 116, 229)',
          color: 'rgb(11, 116, 229)',
          width: '240px',
          height: '38px',
          borderRadius: '4px'
        }}
        styleTextButton={{ fontWeight: 500 }} />
      </div>
    </div>
  </div>
    </>
  )
}

export default HomePage
