import Head from 'next/head'
import { FC, useEffect, useState } from "react"
import ClientLayout from '@/components/structure/ClientLayout'
import { GetStaticProps, GetServerSideProps } from 'next'
import DishCartCard from '@/components/cards/DishCartCard'
import { useCookies } from 'react-cookie'


// export const getServerSideProps:GetServerSideProps = async () =>{
//     const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
//     const categories = await resp1.json() as categoryItem[];

// const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCart`, {
//   credentials: 'include'
// });
// const cartDishes = await resp.json() as dishClientCardProps[];

//     return {
//       props:{
//         //dishes: cartDishes,
//         categories: categories,
//       }
//     }
//   }

export const getStaticProps:GetStaticProps = async () =>{
  const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
  const categories = await resp.json() as categoryItem[];

  return {
    props:{
      categories
    }
  }
}

const Cart: FC<{categories:categoryItem[]}> = ({categories}) => {
    const [dishes, setDishes] = useState<dishClientInfo[]>([]);
    const [cookies, setCookie, removeCookie] = useCookies(['cartDishes']);

    useEffect(() => {
        const fetchData = async () =>{
        const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCart`, {
          credentials: 'include'
        });
        const cartDishes = await resp.json() as dishClientInfo[];
        setDishes(cartDishes);
    }
    fetchData();
    }, [])

  const handleDeleteItem = (dishId:string):void => {
    setDishes(prevState => prevState.filter(el => el.id != dishId ));
  }

  const clearList = ():void =>{
    setDishes([]);
    removeCookie('cartDishes');
  }

  const handlePlaceAnOrder = async () => {

    const response = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/placeAnOrder`, {
        method: "POST",
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        }
    });

    if(response.ok){
      clearList();
    }
    else{
      alert("Заказ не был оформлен")
    }
  }

  return (
    <ClientLayout categories={categories}>
      <Head>
        <title>Create Next App</title>
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <link rel="icon" href="/favicon.ico" />
      </Head>
      <main className='mb-2'>
        <div>
          {dishes.map((dish, i) => <DishCartCard key={i} {...dish} DeleteCartFromList={handleDeleteItem}/>)}
        </div>
        {dishes.length !=0 ? 
          (<div>
            <button className='btn btn-primary w-100 mt-2' onClick={handlePlaceAnOrder}>
              Оформить заказ
            </button>
          </div>)
          : (
            <div className='d-flex justify-content-center mt-5'>
              <h2>
                Ваша корзина пуста
              </h2>
            </div>
          )
        }
      </main>
    </ClientLayout>
  )
}

export default Cart;