<?php

namespace App\Controller;

use Symfony\Component\Routing\Annotation\Route;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use App\Entity\Product;
use App\Form\ProductType;
use Doctrine\ORM\EntityManagerInterface;
use Symfony\Component\Validator\Validator\ValidatorInterface;
use Symfony\Component\Form\Extension\Core\Type\SubmitType;
use Symfony\Component\Form\Extension\Core\Type\TextType;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\HttpFoundation\Request;


class ProductController extends AbstractController
{
    
    /**
     * @Route("/product", name="create_product")
     */
//     public function createProduct(): Response
//     {
//         // you can fetch the EntityManager via $this->getDoctrine()
//         // or you can add an argument to the action: createProduct(EntityManagerInterface $entityManager)
//         $entityManager = $this->getDoctrine()->getManager();
        
//         $product = new Product();
//         $product->setName('Keyboard');
//         $product->setPrice(1999);
//         $product->setDescription('Ergonomic and stylish!');
        
//         //Validate if the Product contains the right information in its fields.
//         $errors = $validator->validate($product);
//         if (count($errors) > 0) {
//             return new Response((string) $errors, 400);
//         }
        
//         // tell Doctrine you want to (eventually) save the Product (no queries yet)
//         $entityManager->persist($product);
        
//         // actually executes the queries (i.e. the INSERT query)
//         $entityManager->flush();
        
//         return new Response('Saved new product with id '.$product->getId());
//     }
    
    /**
     * @Route("/product/{id}", name="product_show")
     */
    public function show($id)
    {
        $product = $this->getDoctrine()
        ->getRepository(Product::class)
        ->find($id);
        
        if (!$product) {
            throw $this->createNotFoundException(
                'No product found for id '.$id
                );
        }
        
        return new Response('Product Name: '.$product->getName());
        
        // or render a template
        // in the template, print things with {{ product.name }}
        // return $this->render('product/show.html.twig', ['product' => $product]);
    }
    
    /**
     * @Route("/products", name="products_show")
     */
    public function showProducts()
    {
        $repository = $this->getDoctrine()->getRepository(Product::class);
        $products = $repository->findAll();
        
        return $this->render(
            'product/products.html.twig',
            array('products' => $products)
            );
    }
    
     /**
      * Displays a form to edit an existing Post entity.
      *
      * @Route("product/{id<\d+>}/edit", name="product_edit")
      */
//     public function edit(int $id)
//     {
//         $product = $this->getDoctrine()
//         ->getRepository(Product::class)
//         ->find($id);
        
//          return $this->render('product/edit_product.html.twig', [
//              'id' => $id,
//              'name' => $product->getName(),
//              'price' => $product->getPrice(),
//              'description' => $product->getDescription()
//          ]);
//     }

    /**
     * Displays a form to edit an existing Product entity.
     *
     * @Route("product/edit/{id<\d+>}", methods="GET|POST", name="product_edit")
     */
    public function edit(Request $request, Product $product): Response
    {
        //Using Forms.
        $form = $this->createForm(ProductType::class, $product);
        
        $form->handleRequest($request);
        
        if ($form->isSubmitted() && $form->isValid()) 
        {
            // $form->getData() holds the submitted values
            // but, the original `$product` variable has also been updated
            $product = $form->getData();
            
            // ... perform some action, such as saving the task to the database
            // for example, if Task is a Doctrine entity, save it!
            $entityManager = $this->getDoctrine()->getManager();
            $entityManager->persist($product);
            $entityManager->flush();
            
            // Flash messages are used to notify the user about the result of the
            // actions. They are deleted automatically from the session as soon
            // as they are accessed.
            // See https://symfony.com/doc/current/controller.html#flash-messages
            $this->addFlash('success', 'product.saved_successfully');
            
            //Redirect to another Action in same Controller.
            return $this->redirectToRoute('products_show');
        }
    
        return $this->render('product/edit_product_form.html.twig', [
            'form' => $form->createView(),
        ]);
    }
    
    /**
     * Saves the Product entity.
     *
     * @Route("product/save}", name="product_save")
     */
    public function save(Request $request, Product $product): Response
    {
        //Using Forms.
        $form = $this->createForm(ProductType::class, $product);
        
        $form->handleRequest($request);
        
        if ($form->isSubmitted() && $form->isValid())
        {
            // $form->getData() holds the submitted values
            // but, the original `$product` variable has also been updated
            $product = $form->getData();
            
            // ... perform some action, such as saving the task to the database
            // for example, if Task is a Doctrine entity, save it!
            $entityManager = $this->getDoctrine()->getManager();
            $entityManager->persist($product);
            $entityManager->flush();
        }
        
        //Redirect to another Action in same Controller.
        return $this->redirectToRoute('products_show');
    }
    
    /**
     * Creates a new Product entity.
     *
     * @Route("product", methods="GET|POST", name="product_create")
     *
     * NOTE: the Method annotation is optional, but it's a recommended practice
     * to constraint the HTTP methods each controller responds to (by default
     * it responds to all methods).
     */
    public function new(Request $request): Response
    {
        $product = new Product();
        
        // See https://symfony.com/doc/current/form/multiple_buttons.html
        $form = $this->createForm(ProductType::class, $product);
        
        $form->handleRequest($request);
        
        // the isSubmitted() method is completely optional because the other
        // isValid() method already checks whether the form is submitted.
        // However, we explicitly add it to improve code readability.
        // See https://symfony.com/doc/current/forms.html#processing-forms
        if ($form->isSubmitted() && $form->isValid()) 
        {
            $em = $this->getDoctrine()->getManager();
            $em->persist($product);
            $em->flush();
            
            // Flash messages are used to notify the user about the result of the
            // actions. They are deleted automatically from the session as soon
            // as they are accessed.
            // See https://symfony.com/doc/current/controller.html#flash-messages
            $this->addFlash('success', 'product.created_successfully');
            
            //Redirect to another Action in same Controller.
            return $this->redirectToRoute('products_show');
        }
        
        return $this->render('product/edit_product_form.html.twig', [
            'form' => $form->createView(),
        ]);
    }
    
    /**
     * Saves a Product.
     *
     * @Route("product/save/{id<\d+>}", name="save_product")
     */
//     public function save(int $id)
//     {
//         $product = $this->getDoctrine()
//         ->getRepository(Product::class)
//         ->find($id);
        
//         //$product->setName($name);
//         //$product->setPrice($price);
//         //$product->setDescription($description);
        
//         //Save the Product into the Database.
//         //SaveProduct($product);
        
//         // you can fetch the EntityManager via $this->getDoctrine()
//         // or you can add an argument to the action: createProduct(EntityManagerInterface $entityManager)
//         $entityManager = $this->getDoctrine()->getManager();
        
//         // tell Doctrine you want to (eventually) save the Product (no queries yet)
//         $entityManager->persist($product);
        
//         // actually executes the queries (i.e. the INSERT query)
//         $entityManager->flush();
        
//         return $this->showProducts();
//     }
    
    private function SaveProduct(Product $product)
    {
        // you can fetch the EntityManager via $this->getDoctrine()
        // or you can add an argument to the action: createProduct(EntityManagerInterface $entityManager)
        $entityManager = $this->getDoctrine()->getManager();
        
        // tell Doctrine you want to (eventually) save the Product (no queries yet)
        $entityManager->persist($product);
        
        // actually executes the queries (i.e. the INSERT query)
        $entityManager->flush();
    }
}
