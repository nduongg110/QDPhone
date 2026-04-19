import React, { Fragment } from 'react'
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import DefaultComponent from './components/DefaultComponent/DefaultComponent'
import { routes } from './routes'

import axios from 'axios'
import { useQuery } from '@tanstack/react-query'

function App() {

  // 🔥 FETCH API
  const fetchApi = async () => {
    const res = await axios.get(
      `${process.env.REACT_APP_API_URL}/product/get-all?limit=10&page=1`
    )
    return res.data
  }

  // 🔥 REACT QUERY
  const { data, isLoading, error } = useQuery({
    queryKey: ['products'],
    queryFn: fetchApi
  })

  // 🔥 LOG TEST
  console.log('ENV:', process.env.REACT_APP_API_URL)
  console.log('LOADING:', isLoading)
  console.log('ERROR:', error)
  console.log('DATA FULL:', data)
  console.log('DATA ARRAY:', data?.data)

  return (
    <Router>
      <Routes>
        {routes.map((route) => {
          const Page = route.page
          const Layout = route.isShowHeader ? DefaultComponent : Fragment

          return (
            <Route
              key={route.path}
              path={route.path}
              element={
                <Layout>
                  <Page />
                </Layout>
              }
            />
          )
        })}
      </Routes>
    </Router>
  )
}

export default App