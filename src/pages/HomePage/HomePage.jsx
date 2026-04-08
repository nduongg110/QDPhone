import React from 'react'
import TypeProduct from '../../components/TypeProduct/TypeProduct'
import { WrapperTypeProduct } from './style'
import banner1 from '../../assets/images/banner1.webp'
import banner2 from '../../assets/images/banner2.jpg'
import banner3 from '../../assets/images/banner3.jpg'
import banner4 from '../../assets/images/banner4.jpg'
import SlideComponent from '../../components/SliderComponent/SliderComponent'

const HomePage = () => {
  const arr = ['Điện thoại thông minh', 'Máy tính bảng', 'Phụ kiện điện thoại']
  return (
    <>
    <div style={{ padding: '0 120px' }}>
      <WrapperTypeProduct>
        {arr.map((item) => {
          return (
            <TypeProduct name={item} key={item} />
          )
        })}
      </WrapperTypeProduct>
    </div>

    <div id="container" style={{ backgroundColor: '#efefef', padding: '0 120px' }}>
      <SlideComponent arrImages={[banner1, banner2, banner3, banner4]} />
    </div>
    </>
  )
}

export default HomePage
